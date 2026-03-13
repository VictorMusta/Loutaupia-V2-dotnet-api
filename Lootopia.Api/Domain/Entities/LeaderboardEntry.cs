namespace Lootopia.Api.Domain.Entities;

public class LeaderboardEntry
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string Scope { get; set; } = "global"; // "global" or "hunt:{huntId}"
    public string Period { get; set; } = "all"; // day, week, month, all
    public string Metric { get; set; } = "points"; // points, hunts_completed, time
    public decimal Score { get; set; }
    public int Rank { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    public User Player { get; set; } = null!;
}
