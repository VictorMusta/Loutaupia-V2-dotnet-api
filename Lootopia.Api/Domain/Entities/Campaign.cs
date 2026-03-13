using Lootopia.Api.Domain.Enums;

namespace Lootopia.Api.Domain.Entities;

public class Campaign
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? HuntId { get; set; }
    public decimal TokenBudget { get; set; }
    public int CouponsDistributed { get; set; }
    public int MaxCoupons { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Partner Partner { get; set; } = null!;
    public Hunt? Hunt { get; set; }
}
