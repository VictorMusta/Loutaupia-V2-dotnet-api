using System;
using System.Threading;
using System.Threading.Tasks;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;

public interface IInventoryRepository
{
    Task<Result<Inventory>> GetByIdAsync(Guid inventoryId, CancellationToken cancellationToken = default);
    Task<Result<Inventory>> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task<Result<Inventory>> CreateAsync(Inventory inventory, CancellationToken cancellationToken = default);
    Task<Result<Inventory>> UpdateAsync(Inventory inventory, CancellationToken cancellationToken = default);
}
