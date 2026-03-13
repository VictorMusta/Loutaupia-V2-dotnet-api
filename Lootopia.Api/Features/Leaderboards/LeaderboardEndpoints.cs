using System.Security.Claims;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
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

        group.MapGet("/", GetLeaderboard)
            .WithName("GetLeaderboard")
            .AllowAnonymous();

        group.MapGet("/me", GetMyRank)
            .WithName("GetMyRank")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetLeaderboard(
        string? scope,
        string? period,
        string? metric,
        int? page,
        int? size,
        ILeaderboardService leaderboardService,
        LootopiaDbContext db,
        IMemoryCache cache,
        CancellationToken cancellationToken)
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
                .Select(e => new LeaderboardRowDto(e.Rank, e.Player!.DisplayName, e.Score))
                .ToListAsync(cancellationToken);

            return new LeaderboardResponse(entries.Count, entries);
        });

        if (result is null)
            return HttpResults.Json(new { Code = "Leaderboard.Error", Description = "Failed to load leaderboard." }, statusCode: 500);

        var skipped = (pageNum - 1) * pageSize;
        var paged = result.Items.Skip(skipped).Take(pageSize).ToList();

        return HttpResults.Ok(new
        {
            result.TotalCount,
            Page = pageNum,
            Size = pageSize,
            Items = paged
        });
    }

    private static async Task<IResult> GetMyRank(
        string? scope,
        string? period,
        string? metric,
        HttpContext httpContext,
        ILeaderboardService leaderboardService,
        LootopiaDbContext db,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext.User);
        if (userId is null)
            return HttpResults.Json(new { Code = "Auth.Unauthorized", Description = "Authentication required." }, statusCode: 401);

        var s = string.IsNullOrEmpty(scope) ? "global" : scope;
        var p = string.IsNullOrEmpty(period) ? "all" : period;
        var m = string.IsNullOrEmpty(metric) ? "points" : metric;

        await leaderboardService.RecalculateAsync(s, p, m, cancellationToken);

        var entry = await db.LeaderboardEntries
            .Include(e => e.Player)
            .FirstOrDefaultAsync(e =>
                e.Scope == s && e.Period == p && e.Metric == m && e.PlayerId == userId.Value,
                cancellationToken);

        if (entry is null)
            return HttpResults.Ok(new { Rank = (int?)null, Score = (decimal?)null, DisplayName = "" });

        return HttpResults.Ok(new
        {
            Rank = entry.Rank,
            Score = entry.Score,
            DisplayName = entry.Player?.DisplayName ?? ""
        });
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

internal sealed record LeaderboardRowDto(int Rank, string DisplayName, decimal Score);

internal sealed record LeaderboardResponse(int TotalCount, List<LeaderboardRowDto> Items);
