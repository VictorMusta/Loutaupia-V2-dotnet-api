using System.Security.Claims;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Auth.MagicLink;

public static class MagicLinkEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/magic-link/generate", async (GenerateMagicLinkRequest request, HttpContext httpContext, IMediator mediator) =>
        {
            var adminIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
                return HttpResults.Json(new { Code = "Auth.Unauthorized", Description = "Authentication required." }, statusCode: 401);

            var result = await mediator.Send(new GenerateMagicLinkCommand(request.PartnerUserId, adminId));
            return result.ToHttpResult();
        })
        .WithTags("Auth")
        .RequireAuthorization("Admin")
        .WithName("GenerateMagicLink");

        app.MapPost("/api/auth/magic-link/validate", async (ValidateMagicLinkRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new ValidateMagicLinkCommand(request.Token));
            return result.ToHttpResult();
        })
        .WithTags("Auth")
        .AllowAnonymous()
        .WithName("ValidateMagicLink");
    }

    private record GenerateMagicLinkRequest(Guid PartnerUserId);
    private record ValidateMagicLinkRequest(string Token);
}
