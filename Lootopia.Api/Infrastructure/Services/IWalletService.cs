using Lootopia.Api.SharedKernel.Results;

namespace Lootopia.Api.Infrastructure.Services;

public interface IWalletService
{
    Task<Result> CreditAsync(
        Guid userId,
        decimal amount,
        string reason,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    Task<Result> DebitAsync(
        Guid userId,
        decimal amount,
        string reason,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    Task<Result> TransferAsync(
        Guid fromUserId,
        Guid toUserId,
        decimal amount,
        string reason,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);
}
