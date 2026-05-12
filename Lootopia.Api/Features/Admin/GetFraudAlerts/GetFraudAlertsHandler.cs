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

        var alertsData = await db.FraudAlerts
            .OrderByDescending(f => f.CreatedAt)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var userIds = alertsData
            .Where(a => a.RelatedUserId.HasValue)
            .Select(a => a.RelatedUserId!.Value)
            .Distinct()
            .ToList();

        var userNames = await db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);

        var items = alertsData.Select(f => new FraudAlertDto(
            f.Id,
            f.RelatedUserId ?? Guid.Empty,
            f.RelatedUserId.HasValue && userNames.TryGetValue(f.RelatedUserId.Value, out var name) ? name : "Unknown User",
            f.Type,
            f.Severity,
            f.Description,
            f.Status.ToString(),
            f.CreatedAt)).ToList();

        return Result.Success(new GetFraudAlertsResponse(items, totalCount, request.Page, request.Size));
    }
}
