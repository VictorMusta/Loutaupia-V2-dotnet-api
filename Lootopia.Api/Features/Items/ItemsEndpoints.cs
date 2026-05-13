using System.Security.Claims;
using Lootopia.Api.Features.Items.CreateItem;
using Lootopia.Api.Features.Items.ListItems;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Items;

public static class ItemsEndpoints
{
    public static void MapItemsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/items")
            .WithTags("Items");

        group.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new ListItemsQuery(), cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("ListItems")
        .WithSummary("List all custom items available in the catalog")
        .RequireAuthorization();

        group.MapPost("/", CreateItem)
        .WithName("CreateItem")
        .WithSummary("Create a new item object (Admin only)")
        .RequireAuthorization("Admin");
    }

    private static async Task<IResult> CreateItem(
        CreateItemRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var rarityEnum = Enum.TryParse<Domain.Enums.ItemRarity>(request.Rarity, true, out var r) ? r : Domain.Enums.ItemRarity.Common;
        var typeEnum = Enum.TryParse<Domain.Enums.ItemType>(request.Type, true, out var t) ? t : Domain.Enums.ItemType.Artifact;

        var result = await mediator.Send(new CreateItemCommand(
            request.Name,
            request.Description,
            rarityEnum,
            typeEnum,
            request.ImageUrl,
            request.IsTradeable), cancellationToken);

        return result.IsSuccess
            ? result.ToCreatedHttpResult($"/api/items/{result.Value.ItemId}")
            : result.ToHttpResult();
    }
}

internal sealed record CreateItemRequest(
    string Name,
    string Description,
    string Rarity,
    string Type,
    string? ImageUrl,
    bool IsTradeable);
