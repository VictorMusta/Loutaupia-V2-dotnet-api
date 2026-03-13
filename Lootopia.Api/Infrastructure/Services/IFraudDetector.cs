using Lootopia.Api.SharedKernel.Results;

namespace Lootopia.Api.Infrastructure.Services;

public interface IFraudDetector
{
    Task<Result> CheckForAnomaliesAsync(
        Guid userId,
        double latitude,
        double longitude,
        DateTime timestamp,
        CancellationToken cancellationToken = default);
}
