using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Campaigns.ActivateCampaign;

public record ActivateCampaignCommand(Guid CampaignId, Guid PartnerId) : IRequest<Result>;
