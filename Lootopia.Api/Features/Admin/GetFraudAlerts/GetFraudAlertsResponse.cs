namespace Lootopia.Api.Features.Admin.GetFraudAlerts;

public record GetFraudAlertsResponse(
    IReadOnlyList<FraudAlertDto> Alerts,
    int TotalCount,
    int Page,
    int Size);

public record FraudAlertDto(
    Guid Id,
    string Type,
    string Description,
    Guid? RelatedUserId,
    Guid? RelatedCampaignId,
    string Severity,
    string Status,
    DateTime CreatedAt);
