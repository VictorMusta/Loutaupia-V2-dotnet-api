namespace Lootopia.Api.Domain.Entities;

public class PlayerAchievement
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid AchievementId { get; set; }
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    public User Player { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
}
