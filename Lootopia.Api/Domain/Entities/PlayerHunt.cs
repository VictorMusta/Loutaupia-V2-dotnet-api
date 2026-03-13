using Lootopia.Api.Domain.Enums;

namespace Lootopia.Api.Domain.Entities;

public class PlayerHunt
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid HuntId { get; set; }
    public int CurrentStepOrder { get; set; } = 1;
    public PlayerHuntStatus Status { get; set; } = PlayerHuntStatus.InProgress;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public User Player { get; set; } = null!;
    public Hunt Hunt { get; set; } = null!;
    public ICollection<StepValidation> StepValidations { get; set; } = [];
}
