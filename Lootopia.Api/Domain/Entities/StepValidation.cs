using NetTopologySuite.Geometries;

namespace Lootopia.Api.Domain.Entities;

public class StepValidation
{
    public Guid Id { get; set; }
    public Guid PlayerHuntId { get; set; }
    public Guid StepId { get; set; }
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    public Point PlayerLocation { get; set; } = null!;
    public bool IsValid { get; set; }

    public PlayerHunt PlayerHunt { get; set; } = null!;
    public HuntStep Step { get; set; } = null!;
}
