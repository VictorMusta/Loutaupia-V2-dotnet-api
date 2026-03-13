using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Admin.GetFraudAlerts;

public record GetFraudAlertsQuery(int Page, int Size) : IRequest<Result<GetFraudAlertsResponse>>;
