using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Auctions.CloseAuction;

public sealed class CloseAuctionHandler(
    LootopiaDbContext db,
    IWalletService walletService,
    IInventoryService inventoryService,
    ICommissionService commissionService) : IRequestHandler<CloseAuctionCommand, Result<CloseAuctionResponse>>
{
    private static readonly Guid PlatformUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public async Task<Result<CloseAuctionResponse>> Handle(CloseAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await db.Auctions
            .Include(a => a.Item)
            .Include(a => a.HighestBid)
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction is null)
            return Result.Failure<CloseAuctionResponse>(Error.NotFound);

        if (auction.Status != "Active")
            return Result.Failure<CloseAuctionResponse>(Error.Custom("Auction.AlreadyClosed", "Auction is already closed."));

        var now = DateTime.UtcNow;
        if (now < auction.EndTime)
            return Result.Failure<CloseAuctionResponse>(Error.Custom("Auction.NotEnded", "Auction has not ended yet."));

        if (auction.HighestBid is not null && auction.HighestBid.Amount >= auction.ReservePrice)
        {
            var winnerId = auction.HighestBid.BidderId;
            var bidAmount = auction.HighestBid.Amount;

            var commissionResult = await commissionService.CalculateCommissionAsync(bidAmount, null, cancellationToken);
            if (commissionResult.IsFailure)
                return Result.Failure<CloseAuctionResponse>(commissionResult.Error);

            var (organiserAmount, _, platformAmount, _) = commissionResult.Value;

            var idempotencyKey = $"auction-close-{auction.Id}";

            var debitResult = await walletService.DebitAsync(
                winnerId, bidAmount, "Auction purchase", idempotencyKey, cancellationToken);
            if (debitResult.IsFailure)
                return Result.Failure<CloseAuctionResponse>(debitResult.Error);

            var creditSellerResult = await walletService.CreditAsync(
                auction.SellerId, organiserAmount, "Auction sale", $"{idempotencyKey}-seller", cancellationToken);
            if (creditSellerResult.IsFailure)
                return Result.Failure<CloseAuctionResponse>(creditSellerResult.Error);

            if (platformAmount > 0)
            {
                var creditPlatformResult = await walletService.CreditAsync(
                    PlatformUserId, platformAmount, "Commission: Auction sale", $"{idempotencyKey}-platform", cancellationToken);
                if (creditPlatformResult.IsFailure)
                    return Result.Failure<CloseAuctionResponse>(creditPlatformResult.Error);
            }

            var transferResult = await inventoryService.AddItemAsync(
                winnerId, auction.ItemId, 1, AcquisitionSource.Auction, cancellationToken);
            if (transferResult.IsFailure)
                return Result.Failure<CloseAuctionResponse>(transferResult.Error);

            auction.Status = "Closed";
            await db.SaveChangesAsync(cancellationToken);

            return Result.Success(new CloseAuctionResponse("Closed", winnerId, bidAmount));
        }
        else
        {
            var addBackResult = await inventoryService.AddItemAsync(
                auction.SellerId, auction.ItemId, 1, AcquisitionSource.Trade, cancellationToken);
            if (addBackResult.IsFailure)
                return Result.Failure<CloseAuctionResponse>(addBackResult.Error);

            auction.Status = "NoSale";
            auction.HighestBidId = null;
            await db.SaveChangesAsync(cancellationToken);

            return Result.Success(new CloseAuctionResponse("NoSale", null, null));
        }
    }
}
