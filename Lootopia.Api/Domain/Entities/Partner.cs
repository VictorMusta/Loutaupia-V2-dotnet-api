namespace Lootopia.Api.Domain.Entities;

public class Partner
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal TokenBudget { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Campaign> Campaigns { get; set; } = [];
}
