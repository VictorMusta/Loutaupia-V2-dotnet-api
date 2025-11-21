# Script de création des interfaces de repositories
Write-Host "🔨 Création des interfaces de repositories..." -ForegroundColor Yellow

$files = @{
    "src/Core/Contracts/Repositories/IPlayerRepository.cs" = @"
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
"@

    "src/Core/Contracts/Repositories/IInventoryRepository.cs" = @"
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
"@

    "src/Core/Contracts/Repositories/IArtefactRepository.cs" = @"
using System;
using System.Threading;
using System.Threading.Tasks;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;

public interface IArtefactRepository
{
    Task<Result<Artefact>> GetByIdAsync(Guid artefactId, CancellationToken cancellationToken = default);
    Task<Result<Artefact>> CreateAsync(Artefact artefact, CancellationToken cancellationToken = default);
    Task<Result<Artefact>> UpdateAsync(Artefact artefact, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid artefactId, CancellationToken cancellationToken = default);
}
"@

    "src/Core/Contracts/Repositories/IArtefactDefinitionRepository.cs" = @"
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;

public interface IArtefactDefinitionRepository
{
    Task<Result<ArtefactDefinition>> GetByIdAsync(Guid artefactDefinitionId, CancellationToken cancellationToken = default);
    Task<Result<List<ArtefactDefinition>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<ArtefactDefinition>> CreateAsync(ArtefactDefinition definition, CancellationToken cancellationToken = default);
    Task<Result<ArtefactDefinition>> UpdateAsync(ArtefactDefinition definition, CancellationToken cancellationToken = default);
}
"@

    "src/Core/Contracts/Repositories/ICurrencyWalletRepository.cs" = @"
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
"@

    "src/Core/Contracts/Repositories/IAuctionListingRepository.cs" = @"
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loutaupia_V2_dotnet_api.Core.Domain.Entities;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Core.Contracts.Repositories;

public interface IAuctionListingRepository
{
    Task<Result<AuctionListing>> GetByIdAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<Result<List<AuctionListing>>> GetActiveListingsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<List<AuctionListing>>> GetBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<Result<List<AuctionListing>>> GetExpiredListingsAsync(CancellationToken cancellationToken = default);
    Task<Result<AuctionListing>> CreateAsync(AuctionListing listing, CancellationToken cancellationToken = default);
    Task<Result<AuctionListing>> UpdateAsync(AuctionListing listing, CancellationToken cancellationToken = default);
}
"@
}

foreach ($file in $files.GetEnumerator()) {
    $file.Value | Out-File -FilePath $file.Key -Encoding UTF8
    Write-Host "✓ $($file.Key)" -ForegroundColor Green
}

Write-Host "`n✅ Toutes les interfaces de repositories créées!" -ForegroundColor Green

