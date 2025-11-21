# Script de création des Features Players
Write-Host "`n🚀 CRÉATION DES FEATURES PLAYERS`n" -ForegroundColor Green

Write-Host "1️⃣  CreatePlayer Feature..." -ForegroundColor Yellow

# CreatePlayerRequest
@"
namespace Loutaupia_V2_dotnet_api.Features.Players.CreatePlayer;

public record CreatePlayerRequest(
    string Username,
    string Email,
    string Password
);
"@ | Out-File -FilePath "src/Features/Players/CreatePlayer/CreatePlayerRequest.cs" -Encoding UTF8

# CreatePlayerResponse
@"
using System;

namespace Loutaupia_V2_dotnet_api.Features.Players.CreatePlayer;

public record CreatePlayerResponse(
    Guid PlayerId,
    string Username,
    string Email,
    DateTime CreatedAt,
    string Token
);
"@ | Out-File -FilePath "src/Features/Players/CreatePlayer/CreatePlayerResponse.cs" -Encoding UTF8

# CreatePlayerUseCase
@"
using System.Threading;
using System.Threading.Tasks;
using Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;
using Loutaupia_V2_dotnet_api.Core.Contracts.Services;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Features.Players.CreatePlayer;

public class CreatePlayerUseCase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ICurrencyWalletRepository _walletRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public CreatePlayerUseCase(
        IPlayerRepository playerRepository,
        IInventoryRepository inventoryRepository,
        ICurrencyWalletRepository walletRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _playerRepository = playerRepository;
        _inventoryRepository = inventoryRepository;
        _walletRepository = walletRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<CreatePlayerResponse>> ExecuteAsync(
        CreatePlayerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _playerRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
            return Result<CreatePlayerResponse>.Failure("Username already exists");

        if (await _playerRepository.ExistsByEmailAsync(request.Email, cancellationToken))
            return Result<CreatePlayerResponse>.Failure("Email already exists");

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var player = new Player(request.Username, request.Email, passwordHash);

        var createResult = await _playerRepository.CreateAsync(player, cancellationToken);
        if (!createResult.IsSuccess)
            return Result<CreatePlayerResponse>.Failure(createResult.Error!);

        var inventory = new Inventory(player.PlayerId);
        await _inventoryRepository.CreateAsync(inventory, cancellationToken);

        var wallet = new CurrencyWallet(player.PlayerId, 1000);
        await _walletRepository.CreateAsync(wallet, cancellationToken);

        var token = _jwtService.GenerateToken(player.PlayerId, player.Username, player.Email);

        var response = new CreatePlayerResponse(
            player.PlayerId,
            player.Username,
            player.Email,
            player.CreatedAt,
            token
        );

        return Result<CreatePlayerResponse>.Success(response);
    }
}
"@ | Out-File -FilePath "src/Features/Players/CreatePlayer/CreatePlayerUseCase.cs" -Encoding UTF8

# CreatePlayerEndpoint
@"
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Loutaupia_V2_dotnet_api.Features.Players.CreatePlayer;

public static class CreatePlayerEndpoint
{
    public static void MapCreatePlayerEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/players/register", async (
            CreatePlayerRequest request,
            CreatePlayerUseCase useCase,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.ExecuteAsync(request, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/players/{result.Value!.PlayerId}", result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("RegisterPlayer")
        .WithOpenApi();
    }
}
"@ | Out-File -FilePath "src/Features/Players/CreatePlayer/CreatePlayerEndpoint.cs" -Encoding UTF8

Write-Host "✓ CreatePlayer créé" -ForegroundColor Green

Write-Host "`n2️⃣  AuthenticatePlayer Feature..." -ForegroundColor Yellow

# AuthenticatePlayerRequest
@"
namespace Loutaupia_V2_dotnet_api.Features.Players.AuthenticatePlayer;

public record AuthenticatePlayerRequest(
    string Username,
    string Password
);
"@ | Out-File -FilePath "src/Features/Players/AuthenticatePlayer/AuthenticatePlayerRequest.cs" -Encoding UTF8

# AuthenticatePlayerResponse
@"
using System;

namespace Loutaupia_V2_dotnet_api.Features.Players.AuthenticatePlayer;

public record AuthenticatePlayerResponse(
    Guid PlayerId,
    string Username,
    string Email,
    string Token
);
"@ | Out-File -FilePath "src/Features/Players/AuthenticatePlayer/AuthenticatePlayerResponse.cs" -Encoding UTF8

# AuthenticatePlayerUseCase
@"
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
"@ | Out-File -FilePath "src/Features/Players/AuthenticatePlayer/AuthenticatePlayerUseCase.cs" -Encoding UTF8

# AuthenticatePlayerEndpoint
@"
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Loutaupia_V2_dotnet_api.Features.Players.AuthenticatePlayer;

public static class AuthenticatePlayerEndpoint
{
    public static void MapAuthenticatePlayerEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/players/login", async (
            AuthenticatePlayerRequest request,
            AuthenticatePlayerUseCase useCase,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.ExecuteAsync(request, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Unauthorized();
        })
        .WithName("LoginPlayer")
        .WithOpenApi();
    }
}
"@ | Out-File -FilePath "src/Features/Players/AuthenticatePlayer/AuthenticatePlayerEndpoint.cs" -Encoding UTF8

Write-Host "✓ AuthenticatePlayer créé" -ForegroundColor Green

Write-Host "`n✅ TOUTES LES FEATURES CRÉÉES!" -ForegroundColor Green

$count = (Get-ChildItem -Recurse -Filter "*.cs" | Where-Object { $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\bin\*" }).Count
Write-Host "📊 Total de fichiers C#: $count" -ForegroundColor Cyan

