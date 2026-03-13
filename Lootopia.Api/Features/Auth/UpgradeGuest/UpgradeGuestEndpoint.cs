using System.Security.Claims;
using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Auth.UpgradeGuest;

public static class UpgradeGuestEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/upgrade", async (UpgradeGuestRequest request, HttpContext httpContext, IMediator mediator) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return HttpResults.Json(new { Code = "Auth.Unauthorized", Description = "Authentication required." }, statusCode: 401);

            var result = await mediator.Send(new UpgradeGuestCommand(userId, request.Email, request.Password, request.DisplayName));
            return result.ToHttpResult();
        })
        .WithTags("Auth")
        .RequireAuthorization()
        .WithName("UpgradeGuest");
    }

    private record UpgradeGuestRequest(string Email, string Password, string DisplayName);
}
