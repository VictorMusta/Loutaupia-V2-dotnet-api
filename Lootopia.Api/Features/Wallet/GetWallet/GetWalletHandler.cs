using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Wallet.GetWallet;

public class GetWalletHandler(LootopiaDbContext db)
    : IRequestHandler<GetWalletQuery, Result<GetWalletResponse>>
{
    public async Task<Result<GetWalletResponse>> Handle(
        GetWalletQuery request,
        CancellationToken cancellationToken)
    {
        var wallet = await db.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

        if (wallet is null)
            return Result.Failure<GetWalletResponse>(Error.NotFound);

        return Result.Success(new GetWalletResponse(
            wallet.Balance,
            wallet.Currency,
            wallet.UpdatedAt));
    }
}
