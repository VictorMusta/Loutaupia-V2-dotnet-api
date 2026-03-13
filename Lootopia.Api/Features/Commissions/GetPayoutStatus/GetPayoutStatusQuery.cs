using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Commissions.GetPayoutStatus;

public record GetPayoutStatusQuery(Guid OrganiserId, int Page, int Size) : IRequest<Result<GetPayoutStatusResponse>>;
