namespace Lootopia.Api.Domain.Entities;

public class Achievement
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string Rarity { get; set; } = "Common"; // Common, Rare, Epic, Legendary
    public int PointsValue { get; set; }
    public string RuleType { get; set; } = string.Empty; // HuntsCompleted, SpeedRun, etc.
    public string? RuleConfig { get; set; } // JSON config
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PlayerAchievement> PlayerAchievements { get; set; } = [];
}
