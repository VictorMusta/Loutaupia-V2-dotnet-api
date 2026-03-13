using System.Text.Json;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Infrastructure.Services;

public class AchievementEngine(LootopiaDbContext db) : IAchievementEngine
{
    public async Task EvaluateAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        var achievements = await db.Achievements.ToListAsync(cancellationToken);
        var completedHunts = await db.PlayerHunts
            .Include(ph => ph.Hunt)
            .Where(ph => ph.PlayerId == playerId && ph.Status == PlayerHuntStatus.Completed && ph.CompletedAt != null)
            .ToListAsync(cancellationToken);

        var huntsCompletedCount = completedHunts.Count;
        var bestTimeSeconds = completedHunts
            .Select(ph => (ph.CompletedAt!.Value - ph.StartedAt).TotalSeconds)
            .DefaultIfEmpty(double.MaxValue)
            .Min();

        foreach (var achievement in achievements)
        {
            var alreadyUnlocked = await db.PlayerAchievements
                .AnyAsync(pa => pa.PlayerId == playerId && pa.AchievementId == achievement.Id, cancellationToken);

            if (alreadyUnlocked)
                continue;

            var unlocked = achievement.RuleType switch
            {
                "HuntsCompleted" => EvaluateHuntsCompleted(achievement, huntsCompletedCount),
                "SpeedRun" => EvaluateSpeedRun(achievement, bestTimeSeconds),
                _ => false
            };

            if (unlocked)
            {
                db.PlayerAchievements.Add(new PlayerAchievement
                {
                    Id = Guid.NewGuid(),
                    PlayerId = playerId,
                    AchievementId = achievement.Id,
                    UnlockedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static bool EvaluateHuntsCompleted(Achievement achievement, int count)
    {
        if (string.IsNullOrEmpty(achievement.RuleConfig))
            return false;

        try
        {
            var config = JsonSerializer.Deserialize<HuntsCompletedConfig>(achievement.RuleConfig);
            return config?.RequiredCount <= count;
        }
        catch
        {
            return false;
        }
    }

    private static bool EvaluateSpeedRun(Achievement achievement, double bestTimeSeconds)
    {
        if (string.IsNullOrEmpty(achievement.RuleConfig))
            return false;

        try
        {
            var config = JsonSerializer.Deserialize<SpeedRunConfig>(achievement.RuleConfig);
            return config != null && bestTimeSeconds <= config.MaxSeconds;
        }
        catch
        {
            return false;
        }
    }

    private sealed record HuntsCompletedConfig(int RequiredCount);

    private sealed record SpeedRunConfig(int MaxSeconds);
}
