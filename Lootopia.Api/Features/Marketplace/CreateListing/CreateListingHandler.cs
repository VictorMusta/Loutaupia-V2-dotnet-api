using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Marketplace.CreateListing;

public sealed class CreateListingHandler(LootopiaDbContext db) : IRequestHandler<CreateListingCommand, Result<CreateListingResponse>>
{
    public async Task<Result<CreateListingResponse>> Handle(
        CreateListingCommand request,
        CancellationToken cancellationToken)
    {
        var item = await db.Items.FindAsync([request.ItemId], cancellationToken);
        if (item is null)
            return Result.Failure<CreateListingResponse>(Error.Custom("Marketplace.ItemNotFound", "Item not found."));

        if (!item.IsTradeable)
            return Result.Failure<CreateListingResponse>(Error.Custom("Marketplace.ItemNotTradeable", "Item is not tradeable."));

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var availableQty = await db.PlayerInventories
                .Where(pi => pi.PlayerId == request.SellerId && pi.ItemId == request.ItemId)
                .SumAsync(pi => pi.Quantity, cancellationToken);

            if (availableQty < request.Stock)
                return Result.Failure<CreateListingResponse>(Error.Custom("Marketplace.InsufficientQuantity", "Insufficient item quantity to list."));

            var toRemove = request.Stock;
            var inventories = await db.PlayerInventories
                .Where(pi => pi.PlayerId == request.SellerId && pi.ItemId == request.ItemId)
                .OrderBy(pi => pi.AcquiredAt)
                .ToListAsync(cancellationToken);

            foreach (var inv in inventories)
            {
                if (toRemove <= 0) break;
                var take = Math.Min(inv.Quantity, toRemove);
                inv.Quantity -= take;
                toRemove -= take;
                if (inv.Quantity == 0)
                    db.PlayerInventories.Remove(inv);
            }

            var listing = new Listing
            {
                Id = Guid.NewGuid(),
                SellerId = request.SellerId,
                ItemId = request.ItemId,
                Price = request.Price,
                Stock = request.Stock,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            db.Listings.Add(listing);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(new CreateListingResponse(
                listing.Id,
                listing.ItemId,
                listing.Price,
                listing.Stock,
                listing.CreatedAt));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
