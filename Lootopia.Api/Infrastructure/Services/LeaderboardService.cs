using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Infrastructure.Services;

public class LeaderboardService(LootopiaDbContext db) : ILeaderboardService
{
    public async Task RecalculateAsync(string scope, string period, string metric, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var cutoff = period switch
        {
            "day" => now.AddDays(-1),
            "week" => now.AddDays(-7),
            "month" => now.AddDays(-30),
            _ => (DateTime?)null
        };

        Guid? huntIdFilter = null;
        if (scope.StartsWith("hunt:", StringComparison.Ordinal) && Guid.TryParse(scope.AsSpan(5), out var hid))
            huntIdFilter = hid;

        var completedQuery = db.PlayerHunts
            .Include(ph => ph.Hunt)
            .Where(ph => ph.Status == PlayerHuntStatus.Completed && ph.CompletedAt != null);

        if (huntIdFilter.HasValue)
            completedQuery = completedQuery.Where(ph => ph.HuntId == huntIdFilter.Value);

        if (cutoff.HasValue)
            completedQuery = completedQuery.Where(ph => ph.CompletedAt >= cutoff.Value);

        var playerHunts = await completedQuery.Include(ph => ph.Hunt).ToListAsync(cancellationToken);

        var aggregated = playerHunts
            .GroupBy(ph => ph.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                Points = g.Sum(ph => ph.Hunt!.RewardTokens),
                HuntsCompleted = g.Count(),
                TotalDurationSeconds = g.Sum(ph => (ph.CompletedAt!.Value - ph.StartedAt).TotalSeconds),
                LastCompletedAt = g.Max(ph => ph.CompletedAt)!
            })
            .ToList();

        var ordered = metric == "time"
            ? aggregated.OrderBy(a => a.TotalDurationSeconds).ThenBy(a => a.LastCompletedAt).ToList()
            : aggregated
                .OrderByDescending(a => metric == "points" ? a.Points : a.HuntsCompleted)
                .ThenBy(a => a.TotalDurationSeconds)
                .ThenBy(a => a.LastCompletedAt)
                .ToList();

        // Use a transaction to prevent race conditions
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Check if entries already exist and were recently calculated (within last 30 seconds)
            var existingEntry = await db.LeaderboardEntries
                .Where(e => e.Scope == scope && e.Period == period && e.Metric == metric)
                .OrderByDescending(e => e.CalculatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingEntry != null && (now - existingEntry.CalculatedAt).TotalSeconds < 30)
            {
                // Recent calculation exists, skip to avoid race condition duplicates
                await transaction.RollbackAsync(cancellationToken);
                return;
            }

            await db.LeaderboardEntries
                .Where(e => e.Scope == scope && e.Period == period && e.Metric == metric)
                .ExecuteDeleteAsync(cancellationToken);

            var entries = new List<LeaderboardEntry>();
            for (var i = 0; i < ordered.Count; i++)
            {
                var a = ordered[i];
                var score = metric switch
                {
                    "points" => (decimal)a.Points,
                    "hunts_completed" => a.HuntsCompleted,
                    "time" => (decimal)a.TotalDurationSeconds,
                    _ => (decimal)a.Points
                };

                entries.Add(new LeaderboardEntry
                {
                    Id = Guid.NewGuid(),
                    PlayerId = a.PlayerId,
                    Scope = scope,
                    Period = period,
                    Metric = metric,
                    Score = score,
                    Rank = i + 1,
                    CalculatedAt = now
                });
            }

            if (entries.Count > 0)
            {
                db.LeaderboardEntries.AddRange(entries);
                await db.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
