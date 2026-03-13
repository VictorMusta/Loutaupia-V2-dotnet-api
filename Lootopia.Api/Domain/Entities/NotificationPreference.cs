namespace Lootopia.Api.Domain.Entities;

public class NotificationPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Category { get; set; } = string.Empty; // Achievement, Hunt, Auction, System, Marketing
    public bool IsEnabled { get; set; } = true;

    public User User { get; set; } = null!;
}
