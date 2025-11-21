using System;
using System.Threading;
using System.Threading.Tasks;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;

public interface ICurrencyWalletRepository
{
    Task<Result<CurrencyWallet>> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default);
    Task<Result<CurrencyWallet>> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task<Result<CurrencyWallet>> CreateAsync(CurrencyWallet wallet, CancellationToken cancellationToken = default);
    Task<Result<CurrencyWallet>> UpdateAsync(CurrencyWallet wallet, CancellationToken cancellationToken = default);
}
