namespace Loutaupia_V2_dotnet_api.Features.Players.CreatePlayer;

public record CreatePlayerRequest(
    string Username,
    string Email,
    string Password
);
