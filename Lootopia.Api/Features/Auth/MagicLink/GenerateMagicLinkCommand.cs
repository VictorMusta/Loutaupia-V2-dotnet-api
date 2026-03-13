using Lootopia.Api.SharedKernel.Results;
using MediatR;

namespace Lootopia.Api.Features.Auth.MagicLink;

public record GenerateMagicLinkCommand(Guid PartnerUserId, Guid RequestingAdminId) : IRequest<Result<GenerateMagicLinkResponse>>;

public record GenerateMagicLinkResponse(string Token, DateTime ExpiresAt, string MagicLinkUrl);
