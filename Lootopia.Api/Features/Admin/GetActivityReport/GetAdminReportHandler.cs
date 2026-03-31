using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Features.Admin.GetActivityReport;

public sealed class GetAdminReportHandler(LootopiaDbContext db)
    : IRequestHandler<GetAdminReportQuery, Result<AdminReportResponse>>
{
    public async Task<Result<AdminReportResponse>> Handle(
        GetAdminReportQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await db.Users.CountAsync(cancellationToken);

        var activeHunts = await db.Hunts
            .CountAsync(h => h.Status == HuntStatus.Active, cancellationToken);

        var pendingAlerts = await db.FraudAlerts
            .CountAsync(a => a.Status == FraudAlertStatus.New, cancellationToken);

        var registrations = await db.Users
            .Where(u => u.CreatedAt >= request.From && u.CreatedAt <= request.To)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new DayCount(
                g.Key.ToString("yyyy-MM-dd"),
                g.Count()))
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        var completions = await db.PlayerHunts
            .Where(ph => ph.Status == PlayerHuntStatus.Completed
                         && ph.CompletedAt != null
                         && ph.CompletedAt >= request.From
                         && ph.CompletedAt <= request.To)
            .ToListAsync(cancellationToken);

        var weeklyCompletions = completions
            .GroupBy(ph =>
            {
                var d = ph.CompletedAt!.Value;
                var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
                var week = cal.GetWeekOfYear(d, System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday);
                return $"{d.Year}-W{week:D2}";
            })
            .Select(g => new WeekCount(g.Key, g.Count()))
            .OrderBy(w => w.Week)
            .ToList();

        return Result.Success(new AdminReportResponse(
            totalUsers,
            activeHunts,
            pendingAlerts,
            registrations,
            weeklyCompletions));
    }
}
