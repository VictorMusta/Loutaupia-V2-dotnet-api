using System.Security.Claims;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Achievements;

public static class AchievementEndpoints
{
    public static void MapAchievementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/achievements").WithTags("Achievements");

        group.MapGet("/", GetMyAchievements)
            .WithName("GetMyAchievements")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetMyAchievements(
        string? filter,
        HttpContext httpContext,
        LootopiaDbContext db,
        IAchievementEngine achievementEngine,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext.User);
        if (userId is null)
            return HttpResults.Json(new { Code = "Auth.Unauthorized", Description = "Authentication required." }, statusCode: 401);

        await achievementEngine.EvaluateAsync(userId.Value, cancellationToken);

        var filterMode = string.IsNullOrEmpty(filter) ? "all" : filter.ToLowerInvariant();

        var allAchievements = await db.Achievements.ToListAsync(cancellationToken);
        var playerAchievementIds = await db.PlayerAchievements
            .Where(pa => pa.PlayerId == userId.Value)
            .Select(pa => pa.AchievementId)
            .ToListAsync(cancellationToken);

        var unlockedLookup = playerAchievementIds.ToHashSet();
        var unlockDates = await db.PlayerAchievements
            .Where(pa => pa.PlayerId == userId.Value)
            .ToDictionaryAsync(pa => pa.AchievementId, pa => pa.UnlockedAt, cancellationToken);

        var items = allAchievements
            .Where(a => filterMode switch
            {
                "unlocked" => unlockedLookup.Contains(a.Id),
                "locked" => !unlockedLookup.Contains(a.Id),
                _ => true
            })
            .Select(a => new AchievementDto(
                a.Id,
                a.Name,
                a.Description,
                a.IconUrl,
                a.Rarity,
                a.PointsValue,
                UnlockedAt: unlockDates.TryGetValue(a.Id, out var u) ? u : null))
            .ToList();

        return HttpResults.Ok(new { Items = items });
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

internal sealed record AchievementDto(
    Guid Id,
    string Name,
    string Description,
    string? IconUrl,
    string Rarity,
    int PointsValue,
    DateTime? UnlockedAt);
