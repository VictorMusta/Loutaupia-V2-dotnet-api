using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Admin.FreezeCampaign;

public record FreezeCampaignCommand(Guid CampaignId) : IRequest<Result>;
