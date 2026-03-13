using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Auctions.CreateAuction;

public sealed class CreateAuctionHandler(LootopiaDbContext db) : IRequestHandler<CreateAuctionCommand, Result<CreateAuctionResponse>>
{
    public async Task<Result<CreateAuctionResponse>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Items.FindAsync([request.ItemId], cancellationToken);
        if (item is null)
            return Result.Failure<CreateAuctionResponse>(Error.Custom("Auction.ItemNotFound", "Item not found."));

        if (!item.IsTradeable)
            return Result.Failure<CreateAuctionResponse>(Error.Custom("Auction.ItemNotTradeable", "Item is not tradeable."));

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var inventory = await db.PlayerInventories
                .FirstOrDefaultAsync(pi => pi.PlayerId == request.SellerId && pi.ItemId == request.ItemId, cancellationToken);
            if (inventory is null || inventory.Quantity < 1)
                return Result.Failure<CreateAuctionResponse>(Error.Custom("Auction.ItemNotOwned", "Seller does not own this item."));

            inventory.Quantity -= 1;
            if (inventory.Quantity == 0)
                db.PlayerInventories.Remove(inventory);

            var now = DateTime.UtcNow;
            var auction = new Auction
            {
                Id = Guid.NewGuid(),
                SellerId = request.SellerId,
                ItemId = request.ItemId,
                ReservePrice = request.ReservePrice,
                MinIncrement = request.MinIncrement,
                StartTime = now,
                EndTime = now.AddMinutes(request.DurationMinutes),
                Status = "Active",
                CreatedAt = now
            };

            db.Auctions.Add(auction);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(new CreateAuctionResponse(auction.Id, auction.StartTime, auction.EndTime));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
