namespace Lootopia.Api.Features.Campaigns.GetCampaigns;

public record GetCampaignsResponse(
    IReadOnlyList<CampaignDto> Campaigns,
    int TotalCount,
    int Page,
    int Size);

public record CampaignDto(
    Guid Id,
    Guid PartnerId,
    string Title,
    Guid? HuntId,
    decimal TokenBudget,
    int CouponsDistributed,
    int MaxCoupons,
    string Status,
    DateTime? ActivatedAt,
    DateTime? ExpiresAt,
    DateTime CreatedAt);
