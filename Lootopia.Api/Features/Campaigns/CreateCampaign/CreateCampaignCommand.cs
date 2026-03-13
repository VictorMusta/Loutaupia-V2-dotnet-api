using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Campaigns.CreateCampaign;

public record CreateCampaignCommand(
    Guid PartnerId,
    string Title,
    Guid? HuntId,
    decimal TokenBudget,
    int MaxCoupons,
    DateTime? ExpiresAt) : IRequest<Result<CreateCampaignResponse>>;

public record CreateCampaignResponse(Guid CampaignId);
