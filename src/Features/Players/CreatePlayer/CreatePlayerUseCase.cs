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
