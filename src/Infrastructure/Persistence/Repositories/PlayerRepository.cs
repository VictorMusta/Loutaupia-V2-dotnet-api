using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Infrastructure.Persistence.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly ApplicationDbContext _context;

    public PlayerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Player>> GetByIdAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var player = await _context.Players
                .Include(p => p.Inventory)
                .Include(p => p.Wallet)
                .FirstOrDefaultAsync(p => p.PlayerId == playerId, cancellationToken);

            return player != null 
                ? Result<Player>.Success(player) 
                : Result<Player>.Failure("Player not found");
        }
        catch (Exception ex)
        {
            return Result<Player>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<Player>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var player = await _context.Players
                .Include(p => p.Inventory)
                .Include(p => p.Wallet)
                .FirstOrDefaultAsync(p => p.Username == username, cancellationToken);

            return player != null 
                ? Result<Player>.Success(player) 
                : Result<Player>.Failure("Player not found");
        }
        catch (Exception ex)
        {
            return Result<Player>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<Player>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Email == email, cancellationToken);

            return player != null 
                ? Result<Player>.Success(player) 
                : Result<Player>.Failure("Player not found");
        }
        catch (Exception ex)
        {
            return Result<Player>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Players.AnyAsync(p => p.Username == username, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Players.AnyAsync(p => p.Email == email, cancellationToken);
    }

    public async Task<Result<Player>> CreateAsync(Player player, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Player>.Success(player);
        }
        catch (Exception ex)
        {
            return Result<Player>.Failure($"Failed to create player: {ex.Message}");
        }
    }

    public async Task<Result<Player>> UpdateAsync(Player player, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Players.Update(player);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Player>.Success(player);
        }
        catch (Exception ex)
        {
            return Result<Player>.Failure($"Failed to update player: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var player = await _context.Players.FindAsync(new object[] { playerId }, cancellationToken);
            if (player == null)
                return Result.Failure("Player not found");

            _context.Players.Remove(player);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete player: {ex.Message}");
        }
    }
}
