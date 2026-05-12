namespace Lootopia.Api.Features.Admin.GetFraudAlerts;

public record GetFraudAlertsResponse(
    IReadOnlyList<FraudAlertDto> Items,
    int Total,
    int Page,
    int Size);

public record FraudAlertDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string Type,
    string Severity,
    string Description,
    string Status,
    DateTime CreatedAt);
