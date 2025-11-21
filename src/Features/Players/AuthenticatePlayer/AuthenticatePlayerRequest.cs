namespace Loutaupia_V2_dotnet_api.Features.Players.AuthenticatePlayer;

public record AuthenticatePlayerRequest(
    string Username,
    string Password
);
