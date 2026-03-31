using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Admin.GetActivityReport;

public record GetAdminReportQuery(DateTime From, DateTime To) : IRequest<Result<AdminReportResponse>>;

public record AdminReportResponse(
    int TotalUsers,
    int ActiveHunts,
    int PendingAlerts,
    IReadOnlyList<DayCount> RegistrationsPerDay,
    IReadOnlyList<WeekCount> CompletionsPerWeek);

public record DayCount(string Date, int Count);
public record WeekCount(string Week, int Count);
