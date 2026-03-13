using System.Security.Claims;
using Lootopia.Api.Features.Inventory.GetInventory;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Inventory;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/inventory", async (
            HttpContext httpContext,
            IMediator mediator,
            string? type,
            string? rarity,
            int page = 1,
            int size = 20) =>
        {
            var playerIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(playerIdClaim) || !Guid.TryParse(playerIdClaim, out var playerId))
                return HttpResults.Unauthorized();

            var itemType = Enum.TryParse<Domain.Enums.ItemType>(type, true, out var t) ? t : (Domain.Enums.ItemType?)null;
            var itemRarity = Enum.TryParse<Domain.Enums.ItemRarity>(rarity, true, out var r) ? r : (Domain.Enums.ItemRarity?)null;

            var result = await mediator.Send(new GetInventoryQuery(playerId, itemType, itemRarity, page, size));
            return result.ToHttpResult();
        })
        .WithTags("Inventory")
        .RequireAuthorization()
        .WithName("GetInventory");
    }
}
