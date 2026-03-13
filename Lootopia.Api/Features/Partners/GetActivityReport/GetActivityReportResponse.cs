namespace Lootopia.Api.Features.Partners.GetActivityReport;

public record GetActivityReportResponse(
    int TotalPlayers,
    int CouponsDistributed,
    decimal BudgetRemaining,
    IReadOnlyList<CampaignStatDto> CampaignStats);

public record CampaignStatDto(
    Guid CampaignId,
    string Title,
    string Status,
    int CouponsDistributed,
    int MaxCoupons,
    decimal TokenBudget);
