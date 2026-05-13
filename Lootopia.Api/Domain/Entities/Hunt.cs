using Lootopia.Api.Domain.Enums;

namespace Lootopia.Api.Domain.Entities;

public class Hunt
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public HuntStatus Status { get; set; } = HuntStatus.Draft;
    public Guid CreatedBy { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Difficulty { get; set; } = 1;
    public decimal RewardTokens { get; set; }
    public int MaxWinners { get; set; } = 5;
    public Guid? RewardItemId { get; set; }
    public Item? RewardItem { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<HuntStep> Steps { get; set; } = [];
    public ICollection<PlayerHunt> PlayerHunts { get; set; } = [];
    public Campaign? Campaign { get; set; }
}
