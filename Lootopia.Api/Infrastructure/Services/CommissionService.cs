using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Infrastructure.Services;

public sealed class CommissionService(LootopiaDbContext db) : ICommissionService
{
    public async Task<Result<CommissionResult>> CalculateCommissionAsync(
        decimal transactionAmount,
        Guid? schemaId = null,
        CancellationToken cancellationToken = default)
    {
        if (transactionAmount <= 0)
            return Result.Failure<CommissionResult>(Error.Custom("Commission.InvalidAmount", "Transaction amount must be positive."));

        var schema = schemaId.HasValue
            ? await db.CommissionSchemas.FindAsync([schemaId.Value], cancellationToken)
            : await db.CommissionSchemas.FirstOrDefaultAsync(s => s.IsDefault, cancellationToken);

        schema ??= await db.CommissionSchemas.FirstOrDefaultAsync(cancellationToken);

        decimal platformAmount;
        decimal organiserAmount;
        decimal commissionAmount;

        if (schema is null)
        {
            platformAmount = 0;
            organiserAmount = transactionAmount;
            commissionAmount = 0;
        }
        else
        {
            platformAmount = transactionAmount * schema.PlatformShare;
            organiserAmount = transactionAmount * schema.OrganiserShare;
            commissionAmount = platformAmount;
        }

        return Result.Success(new CommissionResult(
            organiserAmount,
            commissionAmount,
            platformAmount,
            organiserAmount));
    }
}
