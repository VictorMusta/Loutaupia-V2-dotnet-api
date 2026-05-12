using Lootopia.Api.Features.Admin.CreditPartnerBudget;
using Lootopia.Api.Features.Admin.FreezeCampaign;
using Lootopia.Api.Features.Admin.FreezeUser;
using Lootopia.Api.Features.Admin.GetActivityReport;
using Lootopia.Api.Features.Admin.GetFraudAlerts;
using Lootopia.Api.Features.Admin.ListUsers;
using Lootopia.Api.Features.Admin.UnfreezeUser;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Admin;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/fraud-alerts", async (IMediator mediator, int page = 1, int size = 20) =>
        {
            var result = await mediator.Send(new GetFraudAlertsQuery(page, size));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("GetFraudAlerts");

        app.MapPost("/api/admin/users/{userId:guid}/freeze", async (Guid userId, IMediator mediator) =>
        {
            var result = await mediator.Send(new FreezeUserCommand(userId));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("FreezeUser");

        app.MapPost("/api/admin/users/{userId:guid}/unfreeze", async (Guid userId, IMediator mediator) =>
        {
            var result = await mediator.Send(new UnfreezeUserCommand(userId));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("UnfreezeUser");

        app.MapPost("/api/admin/campaigns/{campaignId:guid}/freeze", async (Guid campaignId, IMediator mediator) =>
        {
            var result = await mediator.Send(new FreezeCampaignCommand(campaignId));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("FreezeCampaign");

        app.MapPost("/api/admin/partners/{partnerId:guid}/credit", async (Guid partnerId, CreditRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreditPartnerBudgetCommand(partnerId, request.Amount));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CreditPartnerBudget");

        app.MapGet("/api/admin/users", async (
            [FromQuery] int page,
            [FromQuery] int size,
            [FromQuery] string? search,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new ListUsersQuery(
                page > 0 ? page : 1,
                size > 0 ? Math.Min(size, 100) : 20,
                search));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("ListUsers");

        app.MapGet("/api/admin/report", async (
            [FromQuery] string? from,
            [FromQuery] string? to,
            IMediator mediator) =>
        {
            var dateFrom = DateTime.TryParse(from, out var f) ? DateTime.SpecifyKind(f, DateTimeKind.Utc) : DateTime.UtcNow.AddDays(-30);
            var dateTo = DateTime.TryParse(to, out var t) ? DateTime.SpecifyKind(t, DateTimeKind.Utc).AddDays(1) : DateTime.UtcNow.AddDays(1);
            var result = await mediator.Send(new GetAdminReportQuery(dateFrom, dateTo));
            return result.ToHttpResult();
        })
        .WithTags("Admin")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("GetAdminActivityReport");
    }

    private record CreditRequest(decimal Amount);
}
