using System.Security.Claims;
using Lootopia.Api.Features.Campaigns.ActivateCampaign;
using Lootopia.Api.Features.Campaigns.CreateCampaign;
using Lootopia.Api.Features.Campaigns.GetCampaigns;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Campaigns;

public static class CampaignEndpoints
{
    public static void MapCampaignEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/campaigns/{campaignId:guid}/activate", async (
            Guid campaignId,
            HttpContext httpContext,
            IMediator mediator,
            LootopiaDbContext db) =>
        {
            var userId = GetUserId(httpContext);
            if (!userId.HasValue)
                return HttpResults.Unauthorized();

            var partner = await db.Partners.FirstOrDefaultAsync(p => p.UserId == userId.Value);
            if (partner is null)
                return HttpResults.Json(new { code = "Partner.NotFound", description = "Partner not found." }, statusCode: 403);

            var result = await mediator.Send(new ActivateCampaignCommand(campaignId, partner.Id));
            return result.ToHttpResult();
        })
        .WithTags("Campaigns")
        .RequireAuthorization(policy => policy.RequireRole("Partner"))
        .WithName("ActivateCampaign");

        app.MapPost("/api/campaigns", async (
            CreateCampaignRequest request,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateCampaignCommand(
                request.PartnerId,
                request.Title,
                request.HuntId,
                request.TokenBudget,
                request.MaxCoupons,
                request.ExpiresAt));
            return result.IsSuccess
                ? result.ToCreatedHttpResult($"/api/campaigns/{result.Value.CampaignId}")
                : result.ToHttpResult();
        })
        .WithTags("Campaigns")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CreateCampaign");

        app.MapGet("/api/campaigns", async (
            HttpContext httpContext,
            IMediator mediator,
            int page = 1,
            int size = 20) =>
        {
            if (!httpContext.User.IsInRole("Admin"))
            {
                return HttpResults.Json(new { code = "Forbidden", description = "Admin access required." }, statusCode: 403);
            }

            var result = await mediator.Send(new GetCampaignsQuery(AdminView: true, null, page, size));
            return result.ToHttpResult();
        })
        .WithTags("Campaigns")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("GetCampaignsAdmin");

        app.MapGet("/api/campaigns/mine", async (
            HttpContext httpContext,
            IMediator mediator,
            LootopiaDbContext db,
            int page = 1,
            int size = 20) =>
        {
            var userId = GetUserId(httpContext);
            if (!userId.HasValue)
                return HttpResults.Unauthorized();

            var partner = await db.Partners.FirstOrDefaultAsync(p => p.UserId == userId.Value);
            if (partner is null)
                return HttpResults.Json(new { code = "Partner.NotFound", description = "Partner not found." }, statusCode: 403);

            var result = await mediator.Send(new GetCampaignsQuery(AdminView: false, partner.Id, page, size));
            return result.ToHttpResult();
        })
        .WithTags("Campaigns")
        .RequireAuthorization(policy => policy.RequireRole("Partner"))
        .WithName("GetCampaignsMine");
    }

    private static Guid? GetUserId(HttpContext httpContext)
    {
        var claim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private record CreateCampaignRequest(
        Guid PartnerId,
        string Title,
        Guid? HuntId,
        decimal TokenBudget,
        int MaxCoupons,
        DateTime? ExpiresAt);
}
