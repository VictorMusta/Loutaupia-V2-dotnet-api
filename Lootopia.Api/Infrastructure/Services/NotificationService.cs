using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Infrastructure.Services;

public class NotificationService(LootopiaDbContext db) : INotificationService
{
    public async Task SendAsync(Guid userId, string type, string title, string body, CancellationToken cancellationToken = default)
    {
        var category = MapTypeToCategory(type);
        var preference = await db.NotificationPreferences
            .FirstOrDefaultAsync(np => np.UserId == userId && np.Category == category, cancellationToken);

        if (preference is { IsEnabled: false })
            return;

        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string MapTypeToCategory(string type) => type switch
    {
        "Achievement" => "Achievement",
        "Hunt" => "Hunt",
        "Auction" => "Auction",
        "System" => "System",
        _ => "System"
    };
}
