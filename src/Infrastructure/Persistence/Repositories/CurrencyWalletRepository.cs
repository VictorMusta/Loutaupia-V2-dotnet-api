using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Infrastructure.Persistence.Repositories;

public class CurrencyWalletRepository : ICurrencyWalletRepository
{
    private readonly ApplicationDbContext _context;

    public CurrencyWalletRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CurrencyWallet>> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        try
        {
            var wallet = await _context.CurrencyWallets
                .FirstOrDefaultAsync(w => w.WalletId == walletId, cancellationToken);

            return wallet != null
                ? Result<CurrencyWallet>.Success(wallet)
                : Result<CurrencyWallet>.Failure("Wallet not found");
        }
        catch (Exception ex)
        {
            return Result<CurrencyWallet>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<CurrencyWallet>> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var wallet = await _context.CurrencyWallets
                .FirstOrDefaultAsync(w => w.PlayerId == playerId, cancellationToken);

            return wallet != null
                ? Result<CurrencyWallet>.Success(wallet)
                : Result<CurrencyWallet>.Failure("Wallet not found");
        }
        catch (Exception ex)
        {
            return Result<CurrencyWallet>.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<CurrencyWallet>> CreateAsync(CurrencyWallet wallet, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.CurrencyWallets.Add(wallet);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<CurrencyWallet>.Success(wallet);
        }
        catch (Exception ex)
        {
            return Result<CurrencyWallet>.Failure($"Failed to create wallet: {ex.Message}");
        }
    }

    public async Task<Result<CurrencyWallet>> UpdateAsync(CurrencyWallet wallet, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.CurrencyWallets.Update(wallet);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<CurrencyWallet>.Success(wallet);
        }
        catch (Exception ex)
        {
            return Result<CurrencyWallet>.Failure($"Failed to update wallet: {ex.Message}");
        }
    }
}
