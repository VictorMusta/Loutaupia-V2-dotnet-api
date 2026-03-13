using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Wallet.CreditWallet;

public class CreditWalletHandler(IWalletService walletService)
    : IRequestHandler<CreditWalletCommand, Result>
{
    public async Task<Result> Handle(
        CreditWalletCommand request,
        CancellationToken cancellationToken)
    {
        return await walletService.CreditAsync(
            request.UserId,
            request.Amount,
            request.Reason,
            request.IdempotencyKey,
            cancellationToken);
    }
}
