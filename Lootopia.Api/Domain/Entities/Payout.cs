namespace Lootopia.Api.Domain.Entities;

public class Payout
{
    public Guid Id { get; set; }
    public Guid OrganiserId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Requested"; // Requested, Processing, Settled
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SettledAt { get; set; }

    public User Organiser { get; set; } = null!;
}
