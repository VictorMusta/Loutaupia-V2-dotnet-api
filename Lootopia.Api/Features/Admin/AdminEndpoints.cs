using Lootopia.Api.Features.Admin.CreditPartnerBudget;
using Lootopia.Api.Features.Admin.FreezeCampaign;
using Lootopia.Api.Features.Admin.FreezeUser;
using Lootopia.Api.Features.Admin.GetFraudAlerts;
using Lootopia.Api.Features.Admin.UnfreezeUser;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
    }

    private record CreditRequest(decimal Amount);
}
