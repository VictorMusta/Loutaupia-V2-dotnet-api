using System.Net;
using System.Text.Json;

namespace Lootopia.Api.Infrastructure.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IWebHostEnvironment env) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, env);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, IWebHostEnvironment env)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        object response;
        if (env.IsDevelopment())
        {
            response = new
            {
                Code = "Server.InternalError",
                Description = exception.Message,
                StackTrace = exception.StackTrace
            };
        }
        else
        {
            response = new
            {
                Code = "Server.InternalError",
                Description = "An unexpected error occurred."
            };
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
