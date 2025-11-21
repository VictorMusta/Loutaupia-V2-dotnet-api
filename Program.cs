using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Loutaupia_V2_dotnet_api.Api.Extensions;
using Serilog;
using Serilog.Events;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/loutaupia-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddCorsPolicy(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseSwaggerDocumentation();
    app.UseApplicationMiddleware();
    app.MapApplicationEndpoints();

    // Health check endpoint
    app.MapGet("/", () => new
    {
        service = "Loutaupia V2 API",
        version = "1.0.0",
        status = "running"
    })
    .WithName("HealthCheck");

    Log.Information("Starting Loutaupia V2 API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
