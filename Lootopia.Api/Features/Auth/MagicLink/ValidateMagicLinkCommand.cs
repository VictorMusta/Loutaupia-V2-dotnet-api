using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Auth.MagicLink;

public record ValidateMagicLinkCommand(string Token) : IRequest<Result<ValidateMagicLinkResponse>>;

public record ValidateMagicLinkResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt);
