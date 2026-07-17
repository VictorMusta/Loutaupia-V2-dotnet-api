using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lootopia.Api.Infrastructure.Services;

public sealed class WalletService(LootopiaDbContext db) : IWalletService
{
    public async Task<Result> CreditAsync(
        Guid userId,
        decimal amount,
        string reason,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result.Failure(Error.Custom("Wallet.InvalidAmount", "Amount must be positive."));

        var ownsTransaction = db.Database.CurrentTransaction is null;
        IDbContextTransaction? transaction = null;
        if (ownsTransaction)
            transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
            if (wallet is null)
            {
                wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Balance = 0,
                    Currency = "LTK",
                    UpdatedAt = DateTime.UtcNow
                };
                db.Wallets.Add(wallet);
            }

            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var existing = await db.WalletTransactions
                    .AnyAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);
                if (existing)
                {
                    if (ownsTransaction)
                        await transaction!.CommitAsync(cancellationToken);
                    return Result.Success();
                }
            }

            var tx = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Amount = amount,
                Type = TransactionType.Credit,
                Reason = reason,
                IdempotencyKey = idempotencyKey,
                CreatedAt = DateTime.UtcNow
            };
            db.WalletTransactions.Add(tx);

            wallet.Balance += amount;
            wallet.UpdatedAt = DateTime.UtcNow;

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

    public async Task<Result> DebitAsync(
        Guid userId,
        decimal amount,
        string reason,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result.Failure(Error.Custom("Wallet.InvalidAmount", "Amount must be positive."));

        var ownsTransaction = db.Database.CurrentTransaction is null;
        IDbContextTransaction? transaction = null;
        if (ownsTransaction)
            transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
            if (wallet is null)
                return Result.Failure(Error.Custom("Wallet.NotFound", "Wallet not found."));

            if (wallet.Balance < amount)
                return Result.Failure(Error.Custom("Wallet.InsufficientBalance", "Insufficient balance."));

            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var existing = await db.WalletTransactions
                    .AnyAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);
                if (existing)
                {
                    if (ownsTransaction)
                        await transaction!.CommitAsync(cancellationToken);
                    return Result.Success();
                }
            }

            var tx = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Amount = -amount,
                Type = TransactionType.Debit,
                Reason = reason,
                IdempotencyKey = idempotencyKey,
                CreatedAt = DateTime.UtcNow
            };
            db.WalletTransactions.Add(tx);

            wallet.Balance -= amount;
            wallet.UpdatedAt = DateTime.UtcNow;

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

    public async Task<Result> TransferAsync(
        Guid fromUserId,
        Guid toUserId,
        decimal amount,
        string reason,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result.Failure(Error.Custom("Wallet.InvalidAmount", "Amount must be positive."));

        if (fromUserId == toUserId)
            return Result.Failure(Error.Custom("Wallet.InvalidTransfer", "Cannot transfer to the same user."));

        var ownsTransaction = db.Database.CurrentTransaction is null;
        IDbContextTransaction? transaction = null;
        if (ownsTransaction)
            transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var debitResult = await DebitAsync(fromUserId, amount, reason, idempotencyKey, cancellationToken);
            if (debitResult.IsFailure)
                return debitResult;

            var creditIdempotencyKey = string.IsNullOrEmpty(idempotencyKey)
                ? null
                : $"{idempotencyKey}-credit";

            var creditResult = await CreditAsync(toUserId, amount, reason, creditIdempotencyKey, cancellationToken);
            if (creditResult.IsFailure)
                return creditResult;

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
