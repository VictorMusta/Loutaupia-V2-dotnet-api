using System.Security.Claims;
using Lootopia.Api.Features.Trading.CreateTradeOffer;
using Lootopia.Api.Features.Trading.GetMyTrades;
using Lootopia.Api.Features.Trading.RespondToTrade;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Trading;

public static class TradingEndpoints
{
    public static void MapTradingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/trading")
            .WithTags("Trading")
            .RequireAuthorization();

        group.MapPost("/offers", CreateTradeOffer)
            .WithName("CreateTradeOffer")
            .WithSummary("Create a new trade offer")
            .Produces<CreateTradeOfferResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/offers/{offerId:guid}/respond", RespondToTrade)
            .WithName("RespondToTrade")
            .WithSummary("Accept or refuse a trade offer")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/offers", GetMyTrades)
            .WithName("GetMyTrades")
            .WithSummary("Get my trade offers (as initiator or receiver)")
            .Produces<GetMyTrades.GetMyTradesResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateTradeOffer(
        ClaimsPrincipal user,
        [FromBody] CreateTradeOfferRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        var offered = request.OfferedItems?.Select(o => new TradeItemDto(o.ItemId, o.Quantity, o.TokenAmount)).ToList() ?? [];
        var requested = request.RequestedItems?.Select(r => new TradeItemDto(r.ItemId, r.Quantity, r.TokenAmount)).ToList() ?? [];

        var expiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(7);

        var result = await mediator.Send(new CreateTradeOfferCommand(
            userId.Value,
            request.ReceiverId,
            offered,
            requested,
            expiresAt), cancellationToken);

        return result.IsSuccess
            ? result.ToCreatedHttpResult($"/api/trading/offers/{result.Value.OfferId}")
            : result.ToHttpResult();
    }

    private static async Task<IResult> RespondToTrade(
        Guid offerId,
        ClaimsPrincipal user,
        [FromBody] RespondToTradeRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        var result = await mediator.Send(new RespondToTradeCommand(offerId, userId.Value, request.Action), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetMyTrades(
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        var result = await mediator.Send(new GetMyTradesQuery(userId.Value, status, page, size), cancellationToken);
        return result.ToHttpResult();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private record CreateTradeOfferRequest(
        Guid ReceiverId,
        IReadOnlyList<TradeItemRequest>? OfferedItems,
        IReadOnlyList<TradeItemRequest>? RequestedItems,
        DateTime? ExpiresAt);

    private record TradeItemRequest(Guid? ItemId, int Quantity, decimal TokenAmount);

    private record RespondToTradeRequest(string Action);
}
