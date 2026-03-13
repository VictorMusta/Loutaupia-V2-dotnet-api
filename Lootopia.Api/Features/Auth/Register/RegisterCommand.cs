using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Auth.Register;

public record RegisterCommand(string Email, string Password, string DisplayName) : IRequest<Result<RegisterResponse>>;

public record RegisterResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt);
