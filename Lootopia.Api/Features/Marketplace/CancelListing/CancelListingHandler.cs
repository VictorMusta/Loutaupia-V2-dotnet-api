using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Marketplace.CancelListing;

public sealed class CancelListingHandler(LootopiaDbContext db, IInventoryService inventoryService)
    : IRequestHandler<CancelListingCommand, Result>
{
    public async Task<Result> Handle(CancelListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await db.Listings
            .Include(l => l.Item)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
            return Result.Failure(Error.Custom("Marketplace.ListingNotFound", "Listing not found."));

        if (listing.SellerId != request.UserId)
            return Result.Failure(Error.Forbidden);

        if (listing.Status != "Active")
            return Result.Failure(Error.Custom("Marketplace.ListingNotActive", "Listing is already cancelled or sold."));

        var addResult = await inventoryService.AddItemAsync(
            listing.SellerId,
            listing.ItemId,
            listing.Stock,
            AcquisitionSource.Marketplace,
            cancellationToken);

        if (addResult.IsFailure)
            return addResult;

        listing.Status = "Cancelled";
        listing.Stock = 0;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
