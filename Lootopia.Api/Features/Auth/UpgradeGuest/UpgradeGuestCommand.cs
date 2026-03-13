using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Auth.UpgradeGuest;

public record UpgradeGuestCommand(Guid UserId, string Email, string Password, string DisplayName) : IRequest<Result<UpgradeGuestResponse>>;

public record UpgradeGuestResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt);
