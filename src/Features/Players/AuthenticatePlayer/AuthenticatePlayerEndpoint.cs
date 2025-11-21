﻿using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Loutaupia_V2_dotnet_api.Features.Players.AuthenticatePlayer;

public static class AuthenticatePlayerEndpoint
{
    public static void MapAuthenticatePlayerEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/players/login", async (
            AuthenticatePlayerRequest request,
            AuthenticatePlayerUseCase useCase,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.ExecuteAsync(request, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Unauthorized();
        })
        .WithName("LoginPlayer");
    }
}
