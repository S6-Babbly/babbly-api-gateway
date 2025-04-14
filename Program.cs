using AspNetCoreRateLimit;
using babbly_api_gateway.Aggregators;
using babbly_api_gateway.Middleware;
using babbly_api_gateway.Services;
using Prometheus;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
ConfigureLogging(builder);

// Load environment-specific configuration
var environment = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configure JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

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

// Prometheus metrics endpoint
app.UseMetricServer();
app.UseHttpMetrics();

// Rate limiting
app.UseIpRateLimiting();

// JWT Authentication
app.UseJwtAuthentication();

// Current user extraction
app.UseCurrentUser();

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
