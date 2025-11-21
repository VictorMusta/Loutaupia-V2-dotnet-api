# Script de création de TOUS les fichiers manquants
Write-Host "🔨 Création de tous les fichiers manquants..." -ForegroundColor Yellow

$files = @{
    "src/Core/Domain/Entities/Inventory.cs" = @"
using System;
using System.Collections.Generic;
using System.Linq;
using Loutaupia_V2_dotnet_api.Core.Domain.Exceptions;

namespace Loutaupia_V2_dotnet_api.Core.Domain.Entities;

public class Inventory
{
    private int _maxSlots;
    private readonly List<Artefact> _items = new();

    public Guid InventoryId { get; private set; }
    public Guid PlayerId { get; private set; }

    public int MaxSlots
    {
        get => _maxSlots;
        set
        {
            if (value < 10 || value > 500)
                throw new DomainException("MaxSlots must be between 10 and 500");
            _maxSlots = value;
        }
    }

    public IReadOnlyCollection<Artefact> Items => _items.AsReadOnly();
    public Player? Player { get; set; }

    private Inventory()
    {
    }

    public Inventory(Guid playerId, int maxSlots = 50)
    {
        InventoryId = Guid.NewGuid();
        PlayerId = playerId;
        MaxSlots = maxSlots;
    }

    public void AddItem(Artefact artefact)
    {
        if (_items.Count >= MaxSlots)
            throw new DomainException("Inventory is full");
        _items.Add(artefact);
    }

    public void RemoveItem(Artefact artefact)
    {
        _items.Remove(artefact);
    }
}
"@

    "src/Core/Domain/Entities/Artefact.cs" = @"
using System;
using Loutaupia_V2_dotnet_api.Core.Domain.Exceptions;

namespace Loutaupia_V2_dotnet_api.Core.Domain.Entities;

public class Artefact
{
    private int _quantity;

    public Guid ArtefactId { get; private set; }
    public Guid InventoryId { get; private set; }
    public Guid ArtefactDefinitionId { get; private set; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (value <= 0)
                throw new DomainException("Quantity must be greater than 0");
            _quantity = value;
        }
    }

    public DateTime AcquiredAt { get; private set; }
    public bool IsBound { get; set; }

    public Inventory? Inventory { get; set; }
    public ArtefactDefinition? ArtefactDefinition { get; set; }

    private Artefact()
    {
    }

    public Artefact(Guid inventoryId, Guid artefactDefinitionId, int quantity, bool isBound = false)
    {
        ArtefactId = Guid.NewGuid();
        InventoryId = inventoryId;
        ArtefactDefinitionId = artefactDefinitionId;
        Quantity = quantity;
        IsBound = isBound;
        AcquiredAt = DateTime.UtcNow;
    }
}
"@

    "src/Core/Domain/Entities/ArtefactDefinition.cs" = @"
using System;
using Loutaupia_V2_dotnet_api.Core.Domain.Exceptions;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Core.Domain.Entities;

public class ArtefactDefinition
{
    private string _name = string.Empty;

    public Guid ArtefactDefinitionId { get; private set; }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Name cannot be empty");
            _name = value;
        }
    }

    public string Description { get; set; } = string.Empty;
    public Rarity Rarity { get; set; }
    public ArtefactCategory Category { get; set; }
    public bool IsStackable { get; set; }
    public int MaxStackSize { get; set; }
    public decimal BaseValue { get; set; }

    private ArtefactDefinition()
    {
    }

    public ArtefactDefinition(string name, Rarity rarity, ArtefactCategory category, decimal baseValue)
    {
        ArtefactDefinitionId = Guid.NewGuid();
        Name = name;
        Rarity = rarity;
        Category = category;
        BaseValue = baseValue;
        IsStackable = category == ArtefactCategory.Consumable || category == ArtefactCategory.Material;
        MaxStackSize = IsStackable ? 99 : 1;
    }
}
"@

    "src/Core/Domain/Entities/CurrencyWallet.cs" = @"
using System;
using Loutaupia_V2_dotnet_api.Core.Domain.Exceptions;

namespace Loutaupia_V2_dotnet_api.Core.Domain.Entities;

public class CurrencyWallet
{
    private long _goldCoins;

    public Guid WalletId { get; private set; }
    public Guid PlayerId { get; private set; }

    public long GoldCoins
    {
        get => _goldCoins;
        set
        {
            if (value < 0)
                throw new DomainException("GoldCoins cannot be negative");
            _goldCoins = value;
            LastUpdated = DateTime.UtcNow;
        }
    }

    public DateTime LastUpdated { get; private set; }
    public Player? Player { get; set; }

    private CurrencyWallet()
    {
    }

    public CurrencyWallet(Guid playerId, long initialGold = 0)
    {
        WalletId = Guid.NewGuid();
        PlayerId = playerId;
        GoldCoins = initialGold;
        LastUpdated = DateTime.UtcNow;
    }

    public void AddGold(long amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive");
        GoldCoins += amount;
    }

    public void DeductGold(long amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive");
        if (GoldCoins < amount)
            throw new DomainException("Insufficient funds");
        GoldCoins -= amount;
    }
}
"@

    "src/Core/Domain/Entities/AuctionListing.cs" = @"
using System;
using Loutaupia_V2_dotnet_api.Core.Domain.Exceptions;
using Loutaupia_V2_dotnet_api.Core.Domain.ValueObjects;

namespace Loutaupia_V2_dotnet_api.Core.Domain.Entities;

public class AuctionListing
{
    private decimal _startingPrice;
    private decimal? _buyoutPrice;

    public Guid ListingId { get; private set; }
    public Guid SellerId { get; private set; }
    public Guid ArtefactId { get; private set; }
    public int Quantity { get; set; }

    public decimal StartingPrice
    {
        get => _startingPrice;
        set
        {
            if (value <= 0)
                throw new DomainException("Starting price must be greater than 0");
            _startingPrice = value;
        }
    }

    public decimal? BuyoutPrice
    {
        get => _buyoutPrice;
        set
        {
            if (value.HasValue && value.Value <= StartingPrice)
                throw new DomainException("Buyout price must be greater than starting price");
            _buyoutPrice = value;
        }
    }

    public decimal? CurrentBid { get; set; }
    public Guid? CurrentBidderId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public AuctionStatus Status { get; set; }
    public DateTime CreatedAt { get; private set; }

    public Player? Seller { get; set; }
    public Player? CurrentBidder { get; set; }
    public Artefact? Artefact { get; set; }

    private AuctionListing()
    {
    }

    public AuctionListing(Guid sellerId, Guid artefactId, int quantity, decimal startingPrice, decimal? buyoutPrice, DateTime expiresAt)
    {
        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future");

        ListingId = Guid.NewGuid();
        SellerId = sellerId;
        ArtefactId = artefactId;
        Quantity = quantity;
        StartingPrice = startingPrice;
        BuyoutPrice = buyoutPrice;
        ExpiresAt = expiresAt;
        Status = AuctionStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }
}
"@
}

foreach ($file in $files.GetEnumerator()) {
    $file.Value | Out-File -FilePath $file.Key -Encoding UTF8
    Write-Host "✓ $($file.Key)" -ForegroundColor Green
}

Write-Host "`n✅ Toutes les entités créées!" -ForegroundColor Green

