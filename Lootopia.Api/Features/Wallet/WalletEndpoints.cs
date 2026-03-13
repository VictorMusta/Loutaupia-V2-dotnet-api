using System.Security.Claims;
using Lootopia.Api.Features.Wallet.CreditWallet;
using Lootopia.Api.Features.Wallet.GetTransactions;
using Lootopia.Api.Features.Wallet.GetWallet;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Wallet;

public static class WalletEndpoints
{
    public static void MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wallet")
            .WithTags("Wallet")
            .RequireAuthorization();

        group.MapGet("/", GetWallet)
            .WithName("GetWallet")
            .WithSummary("Get the authenticated user's wallet balance")
            .Produces<GetWalletResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/credit", CreditWallet)
            .WithName("CreditWallet")
            .WithSummary("Credit a user's wallet (Admin only)")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/transactions", GetTransactions)
            .WithName("GetTransactions")
            .WithSummary("Get paginated transaction history")
            .Produces<GetTransactionsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetWallet(
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        var result = await mediator.Send(new GetWalletQuery(userId.Value), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreditWallet(
        [FromBody] CreditWalletRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreditWalletCommand(
            request.UserId,
            request.Amount,
            request.Reason,
            request.IdempotencyKey);

        var result = await mediator.Send(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetTransactions(
        ClaimsPrincipal user,
        [FromServices] IMediator mediator,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        var result = await mediator.Send(
            new GetTransactionsQuery(userId.Value, page, size),
            cancellationToken);

        return result.ToHttpResult();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
