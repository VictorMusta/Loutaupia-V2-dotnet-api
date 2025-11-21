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
