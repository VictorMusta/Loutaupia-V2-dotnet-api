using Lootopia.Api.Domain.Enums;
using Lootopia.Api.SharedKernel.Results;

namespace Lootopia.Api.Infrastructure.Services;

public interface IInventoryService
{
    Task<Result> AddItemAsync(
        Guid playerId,
        Guid itemId,
        int quantity,
        AcquisitionSource source,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveItemAsync(
        Guid playerId,
        Guid itemId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task<Result> TransferItemAsync(
        Guid fromPlayerId,
        Guid toPlayerId,
        Guid itemId,
        int quantity,
        CancellationToken cancellationToken = default);
}
