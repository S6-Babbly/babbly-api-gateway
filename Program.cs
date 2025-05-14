using AspNetCoreRateLimit;
using babbly_api_gateway.Aggregators;
using babbly_api_gateway.Middleware;
using babbly_api_gateway.Services;
using babbly_api_gateway.Transforms;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
ConfigureLogging(builder);

// Load environment-specific configuration
var environment = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

// Configure Auth0 Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Try to get Auth0 settings from environment variables first, then fall back to configuration
    var domain = Environment.GetEnvironmentVariable("AUTH0_DOMAIN") ?? 
                 builder.Configuration["Auth0:Domain"] ?? 
                 throw new InvalidOperationException("Auth0 Domain is not configured.");
                 
    var audience = Environment.GetEnvironmentVariable("AUTH0_AUDIENCE") ?? 
                   builder.Configuration["Auth0:Audience"] ?? 
                   throw new InvalidOperationException("Auth0 Audience is not configured.");
    
    options.Authority = $"https://{domain}/";
    options.Audience = audience;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "https://babbly.com/roles",
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register transform providers
builder.Services.AddSingleton<ForwardUserClaimsTransform>();

// Configure YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<ForwardUserClaimsTransform>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins(
                    builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? 
                    new[] { "http://localhost:3000" }
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// Configure Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// Register HttpClient for services
builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ReverseProxy:Clusters:users-cluster:Destinations:users-service:Address")!);
});

builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ReverseProxy:Clusters:auth-cluster:Destinations:auth-service:Address")!);
});

builder.Services.AddHttpClient("PostService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ReverseProxy:Clusters:posts-cluster:Destinations:posts-service:Address")!);
});

builder.Services.AddHttpClient("CommentService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ReverseProxy:Clusters:comments-cluster:Destinations:comments-service:Address")!);
});

builder.Services.AddHttpClient("LikeService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ReverseProxy:Clusters:likes-cluster:Destinations:likes-service:Address")!);
});

// Register Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILikeService, LikeService>();

// Register Aggregators
builder.Services.AddScoped<FeedAggregator>();
builder.Services.AddScoped<ProfileAggregator>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use Serilog for request logging
app.UseSerilogRequestLogging();

// Enable CORS
app.UseCors("AllowSpecificOrigins");

// Prometheus metrics endpoint
app.UseMetricServer();
app.UseHttpMetrics();

// Rate limiting
app.UseIpRateLimiting();

// Add token validation middleware
app.UseTokenValidation();

// Add auth middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map reverse proxy routes
app.MapReverseProxy();

app.Run();

void ConfigureLogging(WebApplicationBuilder builder)
{
    var environment = builder.Environment;
    var configuration = builder.Configuration;
    
    // Get the application name for the Elasticsearch index
    var applicationName = Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-");
    
    // Create Serilog logger configuration
    var loggerConfig = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithExceptionDetails()
        .Enrich.WithProperty("Environment", environment.EnvironmentName)
        .Enrich.WithProperty("Application", applicationName);
        
    // Configure console and file sinks for all environments
    loggerConfig = loggerConfig
        .WriteTo.Console()
        .WriteTo.File(
            path: $"logs/babbly-api-gateway-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7);
    
    // Configure Elasticsearch only for non-Development environments if specified
    var elasticsearchUrl = configuration["Elasticsearch:Uri"];
    if (!string.IsNullOrEmpty(elasticsearchUrl) && !environment.IsDevelopment())
    {
        loggerConfig = loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
        {
            AutoRegisterTemplate = true,
            IndexFormat = $"{applicationName}-{environment.EnvironmentName.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
            NumberOfShards = 2,
            NumberOfReplicas = 1
        });
    }
    
    // Set Serilog as the logging provider
    Log.Logger = loggerConfig.CreateLogger();
    builder.Host.UseSerilog();
}
