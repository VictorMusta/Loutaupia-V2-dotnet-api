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
