using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Admin.GetFraudAlerts;

public sealed class GetFraudAlertsHandler(LootopiaDbContext db) : IRequestHandler<GetFraudAlertsQuery, Result<GetFraudAlertsResponse>>
{
    public async Task<Result<GetFraudAlertsResponse>> Handle(GetFraudAlertsQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await db.FraudAlerts.CountAsync(cancellationToken);

        var alerts = await db.FraudAlerts
            .OrderByDescending(f => f.CreatedAt)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(f => new FraudAlertDto(
                f.Id,
                f.Type,
                f.Description,
                f.RelatedUserId,
                f.RelatedCampaignId,
                f.Severity,
                f.Status.ToString(),
                f.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetFraudAlertsResponse(alerts, totalCount, request.Page, request.Size));
    }
}
