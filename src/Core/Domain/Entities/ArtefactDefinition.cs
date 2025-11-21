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
