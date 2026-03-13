using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Campaigns.GetCampaigns;

public record GetCampaignsQuery(bool AdminView, Guid? PartnerId, int Page, int Size) : IRequest<Result<GetCampaignsResponse>>;
