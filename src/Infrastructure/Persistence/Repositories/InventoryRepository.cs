using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Infrastructure.Persistence.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Inventory>> GetByIdAsync(Guid inventoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.InventoryId == inventoryId, cancellationToken);

            return inventory != null
                ? Result<Inventory>.Success(inventory)
                : Result<Inventory>.Failure("Inventory not found");
        }
        catch (Exception ex)
        {
            return Result<Inventory>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<Inventory>> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.PlayerId == playerId, cancellationToken);

            return inventory != null
                ? Result<Inventory>.Success(inventory)
                : Result<Inventory>.Failure("Inventory not found");
        }
        catch (Exception ex)
        {
            return Result<Inventory>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<Inventory>> CreateAsync(Inventory inventory, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Inventory>.Success(inventory);
        }
        catch (Exception ex)
        {
            return Result<Inventory>.Failure($"Failed to create inventory: {ex.Message}");
        }
    }

    public async Task<Result<Inventory>> UpdateAsync(Inventory inventory, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Inventory>.Success(inventory);
        }
        catch (Exception ex)
        {
            return Result<Inventory>.Failure($"Failed to update inventory: {ex.Message}");
        }
    }
}
