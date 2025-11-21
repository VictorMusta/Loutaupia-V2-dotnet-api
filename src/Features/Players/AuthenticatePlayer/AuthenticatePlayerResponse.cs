using System;

namespace Loutaupia_V2_dotnet_api.Features.Players.AuthenticatePlayer;

public record AuthenticatePlayerResponse(
    Guid PlayerId,
    string Username,
    string Email,
    string Token
);
