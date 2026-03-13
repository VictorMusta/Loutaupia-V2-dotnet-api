using Lootopia.Api.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.Features.Auth.Register;

public static class RegisterEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (RegisterRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new RegisterCommand(request.Email, request.Password, request.DisplayName));
            return result.ToHttpResult();
        })
        .WithTags("Auth")
        .AllowAnonymous()
        .WithName("Register");
    }

    private record RegisterRequest(string Email, string Password, string DisplayName);
}
