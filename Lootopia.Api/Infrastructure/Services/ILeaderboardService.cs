namespace Lootopia.Api.Infrastructure.Services;

public interface ILeaderboardService
{
    Task RecalculateAsync(string scope, string period, string metric, CancellationToken cancellationToken = default);
}
