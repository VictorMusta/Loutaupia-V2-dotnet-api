using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Auth.GuestLogin;

public static class GuestLoginEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/guest", async (GuestLoginRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new GuestLoginCommand(request.DeviceId));
            return result.ToHttpResult();
        })
        .WithTags("Auth")
        .AllowAnonymous()
        .WithName("GuestLogin");
    }

    private record GuestLoginRequest(string DeviceId);
}
