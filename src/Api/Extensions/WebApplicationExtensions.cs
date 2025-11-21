﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Loutaupia_V2_dotnet_api.Features.Players.CreatePlayer;
using Loutaupia_V2_dotnet_api.Features.Players.AuthenticatePlayer;

namespace Loutaupia_V2_dotnet_api.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        app.MapCreatePlayerEndpoint();
        app.MapAuthenticatePlayerEndpoint();
        return app;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Loutaupia V2 API v1");
            });
        }
        return app;
    }
}
