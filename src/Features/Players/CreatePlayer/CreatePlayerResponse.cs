using System;

namespace Loutaupia_V2_dotnet_api.Features.Players.CreatePlayer;

public record CreatePlayerResponse(
    Guid PlayerId,
    string Username,
    string Email,
    DateTime CreatedAt,
    string Token
);
