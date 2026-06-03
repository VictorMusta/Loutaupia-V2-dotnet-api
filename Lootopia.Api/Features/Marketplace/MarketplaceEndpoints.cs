using System.Security.Claims;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Marketplace.CancelListing;
using Lootopia.Api.Features.Marketplace.CreateListing;
using Lootopia.Api.Features.Marketplace.ListListings;
using Lootopia.Api.Features.Marketplace.PurchaseItem;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Marketplace;

public static class MarketplaceEndpoints
{
    public static void MapMarketplaceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/marketplace")
            .WithTags("Marketplace");

        group.MapPost("/listings", CreateListing)
            .WithName("CreateListing")
            .WithSummary("Create a new listing (auth required)")
            .RequireAuthorization()
            .Produces<CreateListing.CreateListingResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/listings/{listingId:guid}/purchase", PurchaseItem)
            .WithName("PurchaseItem")
            .WithSummary("Purchase items from a listing (auth required)")
            .RequireAuthorization()
            .Produces<PurchaseItem.PurchaseItemResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/listings", ListListings)
            .WithName("ListListings")
            .WithSummary("List marketplace listings (public, filterable, sortable, paginated)")
            .AllowAnonymous()
            .Produces<ListListings.ListListingsResponse>(StatusCodes.Status200OK);

        group.MapGet("/listings/mine", ListMyListings)
            .WithName("ListMyListings")
            .WithSummary("List current user's listings")
            .RequireAuthorization()
            .Produces<ListListings.ListListingsResponse>(StatusCodes.Status200OK);

        group.MapPost("/listings/{listingId:guid}/cancel", CancelListing)
            .WithName("CancelListing")
            .WithSummary("Cancel a listing (auth required, owner only)")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateListing(
        ClaimsPrincipal user,
        [FromBody] CreateListingRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        var result = await mediator.Send(new CreateListingCommand(userId.Value, request.ItemId, request.Price, request.Stock), cancellationToken);
        return result.IsSuccess
            ? result.ToCreatedHttpResult($"/api/marketplace/listings/{result.Value.ListingId}")
            : result.ToHttpResult();
    }

    private static async Task<IResult> PurchaseItem(
        Guid listingId,
        ClaimsPrincipal user,
        [FromBody] PurchaseItemRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        var result = await mediator.Send(new PurchaseItemCommand(listingId, userId.Value, request.Quantity, idempotencyKey), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> ListListings(
        [FromServices] IMediator mediator,
        [FromQuery] string? type,
        [FromQuery] string? rarity,
        [FromQuery] string? name,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        var itemType = Enum.TryParse<ItemType>(type, true, out var t) ? t : (ItemType?)null;
        var itemRarity = Enum.TryParse<ItemRarity>(rarity, true, out var r) ? r : (ItemRarity?)null;

        var result = await mediator.Send(
            new ListListingsQuery(itemType, itemRarity, name, minPrice, maxPrice, sort, page, size),
            cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> ListMyListings(
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        var result = await mediator.Send(
            new ListListingsQuery(null, null, null, null, null, "created_desc", page, size, userId),
            cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CancelListing(
        Guid listingId,
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        var result = await mediator.Send(new CancelListingCommand(listingId, userId.Value), cancellationToken);
        return result.ToHttpResult();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private record CreateListingRequest(Guid ItemId, decimal Price, int Stock);
    private record PurchaseItemRequest(int Quantity);
}
