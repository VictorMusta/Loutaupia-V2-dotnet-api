using Lootopia.Api.Domain.Enums;

namespace Lootopia.Api.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string? DeviceId { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Guest;
    public bool IsGuest { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public Wallet? Wallet { get; set; }
    public Partner? Partner { get; set; }
    public ICollection<PlayerHunt> PlayerHunts { get; set; } = [];
    public ICollection<PlayerInventory> PlayerInventories { get; set; } = [];
    public ICollection<Listing> Listings { get; set; } = [];
    public ICollection<TradeOffer> TradeOffersAsInitiator { get; set; } = [];
    public ICollection<TradeOffer> TradeOffersAsReceiver { get; set; } = [];
    public ICollection<Auction> AuctionsAsSeller { get; set; } = [];
    public ICollection<Bid> Bids { get; set; } = [];
    public ICollection<Payout> Payouts { get; set; } = [];
    public ICollection<LeaderboardEntry> LeaderboardEntries { get; set; } = [];
    public ICollection<PlayerAchievement> PlayerAchievements { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<NotificationPreference> NotificationPreferences { get; set; } = [];
}
