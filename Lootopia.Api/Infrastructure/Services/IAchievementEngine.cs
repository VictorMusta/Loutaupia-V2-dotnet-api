namespace Lootopia.Api.Infrastructure.Services;

public interface IAchievementEngine
{
    Task EvaluateAsync(Guid playerId, CancellationToken cancellationToken = default);
}
