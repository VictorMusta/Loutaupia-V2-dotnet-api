using Loutaupia_V2_dotnet_api.Core.Contracts.Services;
namespace Loutaupia_V2_dotnet_api.Infrastructure.Authentication;
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, 12);
    }
    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
