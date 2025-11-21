using System;
using System.Threading;
using System.Threading.Tasks;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;

public interface IPlayerRepository
{
    Task<Result<Player>> GetByIdAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task<Result<Player>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Result<Player>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<Player>> CreateAsync(Player player, CancellationToken cancellationToken = default);
    Task<Result<Player>> UpdateAsync(Player player, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid playerId, CancellationToken cancellationToken = default);
}
