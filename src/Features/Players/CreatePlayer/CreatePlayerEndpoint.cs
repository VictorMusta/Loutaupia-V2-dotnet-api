﻿using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Loutaupia_V2_dotnet_api.Features.Players.CreatePlayer;

public static class CreatePlayerEndpoint
{
    public static void MapCreatePlayerEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/players/register", async (
            CreatePlayerRequest request,
            CreatePlayerUseCase useCase,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.ExecuteAsync(request, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/players/{result.Value!.PlayerId}", result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("RegisterPlayer");
    }
}
