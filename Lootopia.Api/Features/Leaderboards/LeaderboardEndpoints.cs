using System.Security.Claims;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Leaderboards;

public static class LeaderboardEndpoints
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    public static void MapLeaderboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leaderboards").WithTags("Leaderboards");

        group.MapGet("/", async (
            [FromQuery] string? scope,
            [FromQuery] string? period,
            [FromQuery] string? metric,
            [FromQuery] int? page,
            [FromQuery] int? size,
            [FromServices] ILeaderboardService leaderboardService,
            [FromServices] LootopiaDbContext db,
            [FromServices] IMemoryCache cache,
            CancellationToken cancellationToken) =>
        {
            var s = string.IsNullOrEmpty(scope) ? "global" : scope;
            var p = string.IsNullOrEmpty(period) ? "all" : period;
            var m = string.IsNullOrEmpty(metric) ? "points" : metric;
            var pageNum = (page ?? 1) < 1 ? 1 : page ?? 1;
            var pageSize = (size ?? 50) is < 1 or > 100 ? 50 : size ?? 50;

            var cacheKey = $"leaderboard:{s}:{p}:{m}";

            var result = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                await leaderboardService.RecalculateAsync(s, p, m, cancellationToken);

                var entries = await db.LeaderboardEntries
                    .Include(e => e.Player)
                    .Where(e => e.Scope == s && e.Period == p && e.Metric == m)
                    .OrderBy(e => e.Rank)
                    .Select(e => new LeaderboardRowDto(e.Rank, e.PlayerId, e.Player!.DisplayName, e.Score))
                    .ToListAsync(cancellationToken);

                return new LeaderboardResponse(entries.Count, entries);
            });

            if (result is null)
                return HttpResults.Json(new { Code = "Leaderboard.Error", Description = "Failed to load leaderboard." }, statusCode: 500);

            var skipped = (pageNum - 1) * pageSize;
            var paged = result.Items.Skip(skipped).Take(pageSize).ToList();

            return HttpResults.Ok(paged);
        })
            .WithName("GetLeaderboard")
            .AllowAnonymous();

        group.MapGet("/me", async (
            [FromQuery] string? scope,
            [FromQuery] string? period,
            [FromQuery] string? metric,
            HttpContext httpContext,
            [FromServices] ILeaderboardService leaderboardService,
            [FromServices] LootopiaDbContext db,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(httpContext.User);
            if (userId is null)
                return HttpResults.Json(new { Code = "Auth.Unauthorized", Description = "Authentication required." }, statusCode: 401);

            var s = string.IsNullOrEmpty(scope) ? "global" : scope;
            var p = string.IsNullOrEmpty(period) ? "all" : period;
            var m = string.IsNullOrEmpty(metric) ? "points" : metric;

            // Don't recalculate here - the main endpoint already does this with caching
            // If entries don't exist, the main endpoint will create them on first access
            var entry = await db.LeaderboardEntries
                .Include(e => e.Player)
                .FirstOrDefaultAsync(e =>
                    e.Scope == s && e.Period == p && e.Metric == m && e.PlayerId == userId.Value,
                    cancellationToken);

            if (entry is null)
                return HttpResults.Ok(new { Rank = 0, Score = 0m, Total = 0 });

            var total = await db.LeaderboardEntries
                .CountAsync(e => e.Scope == s && e.Period == p && e.Metric == m, cancellationToken);

            return HttpResults.Ok(new
            {
                Rank = entry.Rank,
                Score = entry.Score,
                Total = total
            });
        })
            .WithName("GetMyRank")
            .RequireAuthorization();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

internal sealed record LeaderboardRowDto(int Rank, Guid UserId, string DisplayName, decimal Score);

internal sealed record LeaderboardResponse(int TotalCount, List<LeaderboardRowDto> Items);
