namespace Loutaupia_V2_dotnet_api.Core.Contracts.Services;
public interface IJwtService
{
    string GenerateToken(System.Guid playerId, string username, string email);
    bool ValidateToken(string token);
    System.Guid? GetPlayerIdFromToken(string token);
}
