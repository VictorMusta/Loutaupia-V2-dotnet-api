using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Commissions.RequestPayout;

public record RequestPayoutCommand(Guid OrganiserId, decimal Amount) : IRequest<Result<RequestPayoutResponse>>;
