using System.Security.Claims;
using Lootopia.Api.Features.Auctions.CloseAuction;
using Lootopia.Api.Features.Auctions.CreateAuction;
using Lootopia.Api.Features.Auctions.GetAuction;
using Lootopia.Api.Features.Auctions.ListAuctions;
using Lootopia.Api.Features.Auctions.PlaceBid;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Auctions;

public static class AuctionEndpoints
{
    public static void MapAuctionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auctions", async (CreateAuctionRequest request, HttpContext httpContext, IMediator mediator) =>
        {
            var userId = GetUserId(httpContext);
            if (!userId.HasValue)
                return HttpResults.Unauthorized();

            var result = await mediator.Send(new CreateAuctionCommand(
                userId.Value,
                request.ItemId,
                request.ReservePrice,
                request.MinIncrement,
                request.DurationMinutes));
            return result.IsSuccess
                ? result.ToCreatedHttpResult($"/api/auctions/{result.Value.AuctionId}")
                : result.ToHttpResult();
        })
        .WithTags("Auctions")
        .RequireAuthorization()
        .WithName("CreateAuction");

        app.MapPost("/api/auctions/{auctionId:guid}/bid", async (Guid auctionId, PlaceBidRequest request, HttpContext httpContext, IMediator mediator) =>
        {
            var userId = GetUserId(httpContext);
            if (!userId.HasValue)
                return HttpResults.Unauthorized();

            var result = await mediator.Send(new PlaceBidCommand(auctionId, userId.Value, request.Amount));
            return result.ToHttpResult();
        })
        .WithTags("Auctions")
        .RequireAuthorization()
        .WithName("PlaceBid");

        app.MapGet("/api/auctions/{auctionId:guid}", async (Guid auctionId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAuctionQuery(auctionId));
            return result.ToHttpResult();
        })
        .WithTags("Auctions")
        .AllowAnonymous()
        .WithName("GetAuction");

        app.MapGet("/api/auctions", async (IMediator mediator, string? status, int page = 1, int size = 20) =>
        {
            var result = await mediator.Send(new ListAuctionsQuery(status, page > 0 ? page : 1, size > 0 ? size : 20));
            return result.ToHttpResult();
        })
        .WithTags("Auctions")
        .AllowAnonymous()
        .WithName("ListAuctions");

        app.MapPost("/api/auctions/{auctionId:guid}/close", async (Guid auctionId, HttpContext httpContext, IMediator mediator) =>
        {
            var userId = GetUserId(httpContext);
            if (userId is not { } uid)
                return HttpResults.Unauthorized();
            var result = await mediator.Send(new CloseAuctionCommand(auctionId, uid));
            return result.ToHttpResult();
        })
        .WithTags("Auctions")
        .RequireAuthorization()
        .WithName("CloseAuction");
    }

    private static Guid? GetUserId(HttpContext httpContext)
    {
        var claim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private record CreateAuctionRequest(Guid ItemId, decimal ReservePrice, decimal MinIncrement, int DurationMinutes);
    private record PlaceBidRequest(decimal Amount);
}
