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
