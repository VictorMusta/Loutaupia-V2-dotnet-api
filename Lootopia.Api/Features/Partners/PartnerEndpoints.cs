using System.Security.Claims;
using Lootopia.Api.Features.Partners.GetActivityReport;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Partners;

public static class PartnerEndpoints
{
    public static void MapPartnerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/partners/me/report", async (
            HttpContext httpContext,
            IMediator mediator,
            LootopiaDbContext db,
            DateTime? from,
            DateTime? to) =>
        {
            var userId = GetUserId(httpContext);
            if (!userId.HasValue)
                return HttpResults.Unauthorized();

            var partner = await db.Partners.FirstOrDefaultAsync(p => p.UserId == userId.Value);
            if (partner is null)
                return HttpResults.Json(new { code = "Partner.NotFound", description = "Partner not found." }, statusCode: 403);

            var fromDate = from.HasValue ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) : DateTime.UtcNow.AddDays(-30);
            var toDate = to.HasValue ? DateTime.SpecifyKind(to.Value, DateTimeKind.Utc) : DateTime.UtcNow;

            var result = await mediator.Send(new GetActivityReportQuery(partner.Id, fromDate, toDate));
            return result.ToHttpResult();
        })
        .WithTags("Partners")
        .RequireAuthorization(policy => policy.RequireRole("Partner"))
        .WithName("GetPartnerActivityReport");
    }

    private static Guid? GetUserId(HttpContext httpContext)
    {
        var claim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
