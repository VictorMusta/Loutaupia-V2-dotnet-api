namespace Lootopia.Api.Features.Campaigns.GetCampaigns;

public record GetCampaignsResponse(
    IReadOnlyList<CampaignDto> Campaigns,
    int TotalCount,
    int Page,
    int Size);

public record CampaignDto(
    Guid Id,
    Guid PartnerId,
    string PartnerName,
    string Title,
    string Description,
    decimal Budget,
    decimal Spent,
    string Status,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime CreatedAt,
    int StartedCount,
    int CompletedCount,
    int CouponsDistributed,
    int MaxCoupons,
    double AverageCompletionMinutes,
    IReadOnlyList<DailyTrackingDto> DailyTracking);

public record DailyTrackingDto(string Date, int Explorations);
