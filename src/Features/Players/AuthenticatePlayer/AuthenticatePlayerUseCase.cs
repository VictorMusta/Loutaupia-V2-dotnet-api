using System.Threading;
using System.Threading.Tasks;
using Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;
using Loutaupia_V2_dotnet_api.Core.Contracts.Services;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Features.Players.AuthenticatePlayer;

public class AuthenticatePlayerUseCase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthenticatePlayerUseCase(
        IPlayerRepository playerRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _playerRepository = playerRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthenticatePlayerResponse>> ExecuteAsync(
        AuthenticatePlayerRequest request,
        CancellationToken cancellationToken = default)
    {
        var playerResult = await _playerRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (!playerResult.IsSuccess)
            return Result<AuthenticatePlayerResponse>.Failure("Invalid username or password");

        var player = playerResult.Value!;

        if (!_passwordHasher.VerifyPassword(request.Password, player.PasswordHash))
            return Result<AuthenticatePlayerResponse>.Failure("Invalid username or password");

        if (!player.IsActive)
            return Result<AuthenticatePlayerResponse>.Failure("Account is deactivated");

        player.UpdateLastLogin();
        await _playerRepository.UpdateAsync(player, cancellationToken);

        var token = _jwtService.GenerateToken(player.PlayerId, player.Username, player.Email);

        var response = new AuthenticatePlayerResponse(
            player.PlayerId,
            player.Username,
            player.Email,
            token
        );

        return Result<AuthenticatePlayerResponse>.Success(response);
    }
}
