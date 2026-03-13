using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Trading.RespondToTrade;

public sealed class RespondToTradeHandler(
    LootopiaDbContext db,
    IWalletService walletService,
    IInventoryService inventoryService) : IRequestHandler<RespondToTradeCommand, Result>
{
    public async Task<Result> Handle(RespondToTradeCommand request, CancellationToken cancellationToken)
    {
        var offer = await db.TradeOffers
            .Include(t => t.Items)
            .ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(t => t.Id == request.OfferId, cancellationToken);

        if (offer is null)
            return Result.Failure(Error.Custom("Trading.OfferNotFound", "Trade offer not found."));

        if (offer.ReceiverId != request.UserId)
            return Result.Failure(Error.Forbidden);

        if (offer.Status != "Pending")
            return Result.Failure(Error.Custom("Trading.OfferNotPending", "Trade offer is no longer pending."));

        if (offer.ExpiresAt < DateTime.UtcNow)
        {
            offer.Status = "Expired";
            await db.SaveChangesAsync(cancellationToken);
            return Result.Failure(Error.Custom("Trading.OfferExpired", "Trade offer has expired."));
        }

        if (request.Action.Equals("refuse", StringComparison.OrdinalIgnoreCase))
        {
            offer.Status = "Refused";
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        if (!request.Action.Equals("accept", StringComparison.OrdinalIgnoreCase))
            return Result.Failure(Error.Validation);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var offeredItems = offer.Items.Where(i => i.Side == "Offered").ToList();
            var requestedItems = offer.Items.Where(i => i.Side == "Requested").ToList();

            foreach (var item in offeredItems.Where(i => i.ItemId.HasValue))
            {
                var transferResult = await inventoryService.TransferItemAsync(
                    offer.InitiatorId,
                    offer.ReceiverId,
                    item.ItemId!.Value,
                    item.Quantity,
                    cancellationToken);
                if (transferResult.IsFailure)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return transferResult;
                }

                if (item.TokenAmount > 0)
                {
                    var walletResult = await walletService.TransferAsync(
                        offer.InitiatorId,
                        offer.ReceiverId,
                        item.TokenAmount,
                        $"Trade {offer.Id}: tokens for offered item",
                        null,
                        cancellationToken);
                    if (walletResult.IsFailure)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return walletResult;
                    }
                }
            }

            foreach (var item in requestedItems.Where(i => i.ItemId.HasValue))
            {
                var transferResult = await inventoryService.TransferItemAsync(
                    offer.ReceiverId,
                    offer.InitiatorId,
                    item.ItemId!.Value,
                    item.Quantity,
                    cancellationToken);
                if (transferResult.IsFailure)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return transferResult;
                }

                if (item.TokenAmount > 0)
                {
                    var walletResult = await walletService.TransferAsync(
                        offer.ReceiverId,
                        offer.InitiatorId,
                        item.TokenAmount,
                        $"Trade {offer.Id}: tokens for requested item",
                        null,
                        cancellationToken);
                    if (walletResult.IsFailure)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return walletResult;
                    }
                }
            }

            foreach (var item in offeredItems.Where(i => !i.ItemId.HasValue && i.TokenAmount > 0))
            {
                var walletResult = await walletService.TransferAsync(
                    offer.InitiatorId,
                    offer.ReceiverId,
                    item.TokenAmount,
                    $"Trade {offer.Id}: offered tokens",
                    null,
                    cancellationToken);
                if (walletResult.IsFailure)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return walletResult;
                }
            }

            foreach (var item in requestedItems.Where(i => !i.ItemId.HasValue && i.TokenAmount > 0))
            {
                var walletResult = await walletService.TransferAsync(
                    offer.ReceiverId,
                    offer.InitiatorId,
                    item.TokenAmount,
                    $"Trade {offer.Id}: requested tokens",
                    null,
                    cancellationToken);
                if (walletResult.IsFailure)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return walletResult;
                }
            }

            offer.Status = "Accepted";
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
