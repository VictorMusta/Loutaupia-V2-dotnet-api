namespace Lootopia.Api.Features.Partners.GetActivityReport;

public record GetActivityReportResponse(
    Guid PartnerId,
    string PartnerName,
    decimal TotalBudget,
    decimal TotalSpent,
    int ActiveCampaigns,
    int CouponsDistributed,
    ReportPeriodDto Period,
    int TotalPlayers,
    decimal BudgetRemaining,
    IReadOnlyList<CampaignStatDto> CampaignStats);

public record ReportPeriodDto(string From, string To);

public record CampaignStatDto(
    Guid CampaignId,
    string Title,
    string Status,
    int CouponsDistributed,
    int MaxCoupons,
    decimal TokenBudget);
