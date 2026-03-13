using Lootopia.Api.Domain.Enums;

namespace Lootopia.Api.Domain.Entities;

public class FraudAlert
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? RelatedUserId { get; set; }
    public Guid? RelatedCampaignId { get; set; }
    public string Severity { get; set; } = "Medium";
    public FraudAlertStatus Status { get; set; } = FraudAlertStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
