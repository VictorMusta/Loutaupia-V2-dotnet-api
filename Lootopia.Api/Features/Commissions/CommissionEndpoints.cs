using System.Security.Claims;
using Lootopia.Api.Features.Commissions.CreateCommissionSchema;
using Lootopia.Api.Features.Commissions.GetCommissionSchemas;
using Lootopia.Api.Features.Commissions.GetPayoutStatus;
using Lootopia.Api.Features.Commissions.RequestPayout;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Commissions;

public static class CommissionEndpoints
{
    public static void MapCommissionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/commissions/schemas", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCommissionSchemasQuery());
            return result.ToHttpResult();
        })
        .WithTags("Commissions")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("GetCommissionSchemas");

        app.MapPost("/api/commissions/schemas", async (CreateCommissionSchemaRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateCommissionSchemaCommand(
                request.Type,
                request.Value,
                request.PlatformShare,
                request.OrganiserShare,
                request.IsDefault));
            return result.IsSuccess
                ? result.ToCreatedHttpResult($"/api/commissions/schemas/{result.Value.SchemaId}")
                : result.ToHttpResult();
        })
        .WithTags("Commissions")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CreateCommissionSchema");

        app.MapPost("/api/commissions/payouts", async (RequestPayoutRequest request, HttpContext httpContext, IMediator mediator) =>
        {
            var userId = GetUserId(httpContext);
            if (!userId.HasValue)
                return HttpResults.Unauthorized();

            var result = await mediator.Send(new RequestPayoutCommand(userId.Value, request.Amount));
            return result.ToHttpResult();
        })
        .WithTags("Commissions")
        .RequireAuthorization(policy => policy.RequireRole("Partner"))
        .WithName("RequestPayout");

        app.MapGet("/api/commissions/payouts", async (HttpContext httpContext, IMediator mediator, int page = 1, int size = 20) =>
        {
            var userId = GetUserId(httpContext);
            if (!userId.HasValue)
                return HttpResults.Unauthorized();

            var result = await mediator.Send(new GetPayoutStatusQuery(userId.Value, page, size));
            return result.ToHttpResult();
        })
        .WithTags("Commissions")
        .RequireAuthorization()
        .WithName("GetPayoutStatus");
    }

    private static Guid? GetUserId(HttpContext httpContext)
    {
        var claim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private record CreateCommissionSchemaRequest(string Type, decimal Value, decimal PlatformShare, decimal OrganiserShare, bool IsDefault);
    private record RequestPayoutRequest(decimal Amount);
}
