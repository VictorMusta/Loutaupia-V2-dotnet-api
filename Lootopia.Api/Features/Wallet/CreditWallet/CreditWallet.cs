using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Wallet.CreditWallet;

public record CreditWalletCommand(Guid UserId, decimal Amount, string Reason, string? IdempotencyKey)
    : IRequest<Result>;

public record CreditWalletRequest(Guid UserId, decimal Amount, string Reason, string? IdempotencyKey);
