using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Wallet.GetTransactions;

public class GetTransactionsHandler(LootopiaDbContext db)
    : IRequestHandler<GetTransactionsQuery, Result<GetTransactionsResponse>>
{
    public async Task<Result<GetTransactionsResponse>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var wallet = await db.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

        if (wallet is null)
            return Result.Failure<GetTransactionsResponse>(Error.NotFound);

        var query = db.WalletTransactions
            .AsNoTracking()
            .Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(t => new TransactionDto(
                t.Id,
                t.Amount,
                t.Type.ToString(),
                t.Reason,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetTransactionsResponse(
            items,
            request.Page,
            request.Size,
            totalCount));
    }
}
