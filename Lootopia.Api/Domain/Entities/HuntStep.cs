using Lootopia.Api.Domain.Enums;
using NetTopologySuite.Geometries;

namespace Lootopia.Api.Domain.Entities;

public class HuntStep
{
    public Guid Id { get; set; }
    public Guid HuntId { get; set; }
    public int StepOrder { get; set; }
    public Point Location { get; set; } = null!;
    public double RadiusMeters { get; set; } = 30;
    public string Clue { get; set; } = string.Empty;
    public StepActionType ActionType { get; set; } = StepActionType.Reach;

    public Hunt Hunt { get; set; } = null!;
    public ICollection<StepValidation> Validations { get; set; } = [];
}
