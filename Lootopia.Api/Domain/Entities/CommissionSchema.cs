namespace Lootopia.Api.Domain.Entities;

public class CommissionSchema
{
    public Guid Id { get; set; }
    public string Type { get; set; } = "Percentage"; // Fixed, Percentage, Split
    public decimal Value { get; set; }
    public decimal PlatformShare { get; set; } = 0.05m;
    public decimal OrganiserShare { get; set; } = 0.95m;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
