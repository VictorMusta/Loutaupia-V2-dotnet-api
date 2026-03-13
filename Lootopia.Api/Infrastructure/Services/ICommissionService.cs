using Lootopia.Api.SharedKernel.Results;

namespace Lootopia.Api.Infrastructure.Services;

public record CommissionResult(decimal NetAmount, decimal CommissionAmount, decimal PlatformAmount, decimal OrganiserAmount);

public interface ICommissionService
{
    Task<Result<CommissionResult>> CalculateCommissionAsync(
        decimal transactionAmount,
        Guid? schemaId = null,
        CancellationToken cancellationToken = default);
}
