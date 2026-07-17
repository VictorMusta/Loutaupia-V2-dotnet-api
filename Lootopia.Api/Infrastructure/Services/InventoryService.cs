using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lootopia.Api.Infrastructure.Services;

public sealed class InventoryService(LootopiaDbContext db) : IInventoryService
{
    public async Task<Result> AddItemAsync(
        Guid playerId,
        Guid itemId,
        int quantity,
        AcquisitionSource source,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return Result.Failure(Error.Custom("Inventory.InvalidQuantity", "Quantity must be positive."));

        var item = await db.Items.FindAsync([itemId], cancellationToken);
        if (item is null)
            return Result.Failure(Error.Custom("Inventory.ItemNotFound", "Item not found."));

        var ownsTransaction = db.Database.CurrentTransaction is null;
        IDbContextTransaction? transaction = null;
        if (ownsTransaction)
            transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var existing = await db.PlayerInventories
                .FirstOrDefaultAsync(pi => pi.PlayerId == playerId && pi.ItemId == itemId, cancellationToken);

            if (existing is not null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                db.PlayerInventories.Add(new PlayerInventory
                {
                    Id = Guid.NewGuid(),
                    PlayerId = playerId,
                    ItemId = itemId,
                    Quantity = quantity,
                    Source = source,
                    AcquiredAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync(cancellationToken);
            if (ownsTransaction)
                await transaction!.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch
        {
            if (ownsTransaction)
                await transaction!.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (ownsTransaction && transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    public async Task<Result> RemoveItemAsync(
        Guid playerId,
        Guid itemId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return Result.Failure(Error.Custom("Inventory.InvalidQuantity", "Quantity must be positive."));

        var ownsTransaction = db.Database.CurrentTransaction is null;
        IDbContextTransaction? transaction = null;
        if (ownsTransaction)
            transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var existing = await db.PlayerInventories
                .FirstOrDefaultAsync(pi => pi.PlayerId == playerId && pi.ItemId == itemId, cancellationToken);

            if (existing is null)
                return Result.Failure(Error.Custom("Inventory.ItemNotFound", "Item not in inventory."));

            if (existing.Quantity < quantity)
                return Result.Failure(Error.Custom("Inventory.InsufficientQuantity", "Insufficient quantity."));

            existing.Quantity -= quantity;
            if (existing.Quantity == 0)
                db.PlayerInventories.Remove(existing);

            await db.SaveChangesAsync(cancellationToken);
            if (ownsTransaction)
                await transaction!.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch
        {
            if (ownsTransaction)
                await transaction!.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (ownsTransaction && transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    public async Task<Result> TransferItemAsync(
        Guid fromPlayerId,
        Guid toPlayerId,
        Guid itemId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return Result.Failure(Error.Custom("Inventory.InvalidQuantity", "Quantity must be positive."));

        if (fromPlayerId == toPlayerId)
            return Result.Failure(Error.Custom("Inventory.InvalidTransfer", "Cannot transfer to self."));

        var item = await db.Items.FindAsync([itemId], cancellationToken);
        if (item is null)
            return Result.Failure(Error.Custom("Inventory.ItemNotFound", "Item not found."));

        var ownsTransaction = db.Database.CurrentTransaction is null;
        IDbContextTransaction? transaction = null;
        if (ownsTransaction)
            transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var fromInv = await db.PlayerInventories
                .FirstOrDefaultAsync(pi => pi.PlayerId == fromPlayerId && pi.ItemId == itemId, cancellationToken);
            if (fromInv is null || fromInv.Quantity < quantity)
                return Result.Failure(Error.Custom("Inventory.InsufficientQuantity", "Insufficient quantity to transfer."));

            fromInv.Quantity -= quantity;
            if (fromInv.Quantity == 0)
                db.PlayerInventories.Remove(fromInv);

            var toInv = await db.PlayerInventories
                .FirstOrDefaultAsync(pi => pi.PlayerId == toPlayerId && pi.ItemId == itemId, cancellationToken);
            if (toInv is not null)
            {
                toInv.Quantity += quantity;
            }
            else
            {
                db.PlayerInventories.Add(new PlayerInventory
                {
                    Id = Guid.NewGuid(),
                    PlayerId = toPlayerId,
                    ItemId = itemId,
                    Quantity = quantity,
                    Source = AcquisitionSource.Trade,
                    AcquiredAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync(cancellationToken);
            if (ownsTransaction)
                await transaction!.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch
        {
            if (ownsTransaction)
                await transaction!.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (ownsTransaction && transaction is not null)
                await transaction.DisposeAsync();
        }
    }
}
