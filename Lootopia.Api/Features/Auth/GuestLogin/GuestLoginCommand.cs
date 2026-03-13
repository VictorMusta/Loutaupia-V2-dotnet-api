using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Auth.GuestLogin;

public record GuestLoginCommand(string DeviceId) : IRequest<Result<GuestLoginResponse>>;

public record GuestLoginResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt);
