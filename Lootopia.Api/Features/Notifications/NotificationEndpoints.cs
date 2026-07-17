using System.Security.Claims;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Notifications;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/", ListNotifications)
            .WithName("ListNotifications");

        group.MapPost("/{id:guid}/read", MarkAsRead)
            .WithName("MarkNotificationAsRead");

        group.MapGet("/preferences", GetPreferences)
            .WithName("GetNotificationPreferences");

        group.MapPut("/preferences", UpdatePreferences)
            .WithName("UpdateNotificationPreferences");
    }

    private static async Task<IResult> ListNotifications(
        ClaimsPrincipal user,
        [FromServices] LootopiaDbContext db,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        var notifications = await db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId.Value)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(
                n.Id,
                n.Type,
                n.Title,
                n.Body,
                n.IsRead,
                n.CreatedAt))
            .ToListAsync(cancellationToken);

        return HttpResults.Ok(notifications);
    }

    private static async Task<IResult> MarkAsRead(
        Guid id,
        ClaimsPrincipal user,
        [FromServices] LootopiaDbContext db,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId.Value, cancellationToken);

        if (notification is null)
            return HttpResults.NotFound();

        notification.IsRead = true;
        await db.SaveChangesAsync(cancellationToken);

        return HttpResults.NoContent();
    }

    private static async Task<IResult> GetPreferences(
        ClaimsPrincipal user,
        [FromServices] LootopiaDbContext db,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        var prefs = await db.NotificationPreferences
            .AsNoTracking()
            .Where(p => p.UserId == userId.Value)
            .ToListAsync(cancellationToken);

        return HttpResults.Ok(new NotificationPreferencesDto(
            IsEnabled(prefs, "Hunt"),
            IsEnabled(prefs, "Auction"),
            IsEnabled(prefs, "Achievement"),
            IsEnabled(prefs, "System")));
    }

    private static async Task<IResult> UpdatePreferences(
        ClaimsPrincipal user,
        [FromBody] NotificationPreferencesDto request,
        [FromServices] LootopiaDbContext db,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
            return HttpResults.Unauthorized();

        await UpsertPreference(db, userId.Value, "Hunt", request.HuntUpdates, cancellationToken);
        await UpsertPreference(db, userId.Value, "Auction", request.MarketplaceAlerts, cancellationToken);
        await UpsertPreference(db, userId.Value, "Achievement", request.AchievementAlerts, cancellationToken);
        await UpsertPreference(db, userId.Value, "System", request.SystemMessages, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        return HttpResults.NoContent();
    }

    private static bool IsEnabled(IReadOnlyList<Domain.Entities.NotificationPreference> prefs, string category)
    {
        var pref = prefs.FirstOrDefault(p => p.Category == category);
        return pref?.IsEnabled ?? true;
    }

    private static async Task UpsertPreference(
        LootopiaDbContext db,
        Guid userId,
        string category,
        bool isEnabled,
        CancellationToken cancellationToken)
    {
        var pref = await db.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Category == category, cancellationToken);

        if (pref is null)
        {
            db.NotificationPreferences.Add(new Domain.Entities.NotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = category,
                IsEnabled = isEnabled
            });
        }
        else
        {
            pref.IsEnabled = isEnabled;
        }
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

internal sealed record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt);

internal sealed record NotificationPreferencesDto(
    bool HuntUpdates,
    bool MarketplaceAlerts,
    bool AchievementAlerts,
    bool SystemMessages);
