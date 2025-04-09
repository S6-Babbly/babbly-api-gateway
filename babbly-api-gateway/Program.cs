using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Check if we should use mock services
bool useMockServices = builder.Configuration.GetValue<bool>("MOCK_SERVICES") || 
                      Environment.GetEnvironmentVariable("MOCK_SERVICES")?.ToLower() == "true";

// Configure configuration sources for Ocelot
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Log that we're using mock services
if (useMockServices && builder.Environment.IsDevelopment())
{
    Console.WriteLine("Mock services enabled - API Gateway will handle requests locally");
}

// Add Ocelot with caching
builder.Services.AddOcelot(builder.Configuration)
    .AddCacheManager(x => 
    {
        x.WithDictionaryHandle();
    });

// Add HttpClientFactory for Health Checks
builder.Services.AddHttpClient();

// Add CORS for NextJS frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? 
                new[] { "http://localhost:3000" }) // Default for NextJS dev
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add rate limiting


// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Babbly API Gateway", Version = "v1" });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Babbly API Gateway v1"));
}

// Global error handling
app.UseExceptionHandler("/error");

// Use CORS
app.UseCors();

// Add authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers directly for mock handling first (before Ocelot)
// This lets our MockController intercept requests when using mock services
if (useMockServices)
{
    app.MapControllers();
}

// Health check endpoint
app.MapHealthChecks("/health");

// Configure Ocelot
await app.UseOcelot();

app.Run(); 