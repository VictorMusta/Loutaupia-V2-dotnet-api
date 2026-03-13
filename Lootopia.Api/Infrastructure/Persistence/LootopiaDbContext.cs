using Lootopia.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Infrastructure.Persistence;

public class LootopiaDbContext(DbContextOptions<LootopiaDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<Hunt> Hunts => Set<Hunt>();
    public DbSet<HuntStep> HuntSteps => Set<HuntStep>();
    public DbSet<PlayerHunt> PlayerHunts => Set<PlayerHunt>();
    public DbSet<StepValidation> StepValidations => Set<StepValidation>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<PlayerInventory> PlayerInventories => Set<PlayerInventory>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<FraudAlert> FraudAlerts => Set<FraudAlert>();
    public DbSet<MagicLink> MagicLinks => Set<MagicLink>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<MarketplacePurchase> MarketplacePurchases => Set<MarketplacePurchase>();
    public DbSet<TradeOffer> TradeOffers => Set<TradeOffer>();
    public DbSet<TradeOfferItem> TradeOfferItems => Set<TradeOfferItem>();
    public DbSet<Auction> Auctions => Set<Auction>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<CommissionSchema> CommissionSchemas => Set<CommissionSchema>();
    public DbSet<Payout> Payouts => Set<Payout>();
    public DbSet<LeaderboardEntry> LeaderboardEntries => Set<LeaderboardEntry>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<PlayerAchievement> PlayerAchievements => Set<PlayerAchievement>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
            e.HasIndex(u => u.DeviceId).HasFilter("\"DeviceId\" IS NOT NULL");
            e.Property(u => u.Role).HasConversion<string>();
        });

        modelBuilder.Entity<Wallet>(e =>
        {
            e.HasKey(w => w.Id);
            e.HasOne(w => w.User).WithOne(u => u.Wallet).HasForeignKey<Wallet>(w => w.UserId);
            e.Property(w => w.Balance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<WalletTransaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasOne(t => t.Wallet).WithMany(w => w.Transactions).HasForeignKey(t => t.WalletId);
            e.Property(t => t.Amount).HasPrecision(18, 2);
            e.Property(t => t.Type).HasConversion<string>();
            e.HasIndex(t => t.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL");
        });

        modelBuilder.Entity<Hunt>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Status).HasConversion<string>();
            e.Property(h => h.RewardTokens).HasPrecision(18, 2);
        });

        modelBuilder.Entity<HuntStep>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Hunt).WithMany(h => h.Steps).HasForeignKey(s => s.HuntId);
            e.Property(s => s.Location).HasColumnType("geometry(Point, 4326)");
            e.Property(s => s.ActionType).HasConversion<string>();
            e.HasIndex(s => new { s.HuntId, s.StepOrder }).IsUnique();
        });

        modelBuilder.Entity<PlayerHunt>(e =>
        {
            e.HasKey(ph => ph.Id);
            e.HasOne(ph => ph.Player).WithMany(u => u.PlayerHunts).HasForeignKey(ph => ph.PlayerId);
            e.HasOne(ph => ph.Hunt).WithMany(h => h.PlayerHunts).HasForeignKey(ph => ph.HuntId);
            e.Property(ph => ph.Status).HasConversion<string>();
            e.HasIndex(ph => new { ph.PlayerId, ph.HuntId });
        });

        modelBuilder.Entity<StepValidation>(e =>
        {
            e.HasKey(sv => sv.Id);
            e.HasOne(sv => sv.PlayerHunt).WithMany(ph => ph.StepValidations).HasForeignKey(sv => sv.PlayerHuntId);
            e.HasOne(sv => sv.Step).WithMany(s => s.Validations).HasForeignKey(sv => sv.StepId);
            e.Property(sv => sv.PlayerLocation).HasColumnType("geometry(Point, 4326)");
        });

        modelBuilder.Entity<Item>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Rarity).HasConversion<string>();
            e.Property(i => i.Type).HasConversion<string>();
        });

        modelBuilder.Entity<PlayerInventory>(e =>
        {
            e.HasKey(pi => pi.Id);
            e.HasOne(pi => pi.Player).WithMany(u => u.PlayerInventories).HasForeignKey(pi => pi.PlayerId);
            e.HasOne(pi => pi.Item).WithMany(i => i.PlayerInventories).HasForeignKey(pi => pi.ItemId);
            e.Property(pi => pi.Source).HasConversion<string>();
            e.HasIndex(pi => new { pi.PlayerId, pi.ItemId });
        });

        modelBuilder.Entity<Partner>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.User).WithOne(u => u.Partner).HasForeignKey<Partner>(p => p.UserId);
        });

        modelBuilder.Entity<Campaign>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Partner).WithMany(p => p.Campaigns).HasForeignKey(c => c.PartnerId);
            e.HasOne(c => c.Hunt).WithOne(h => h.Campaign).HasForeignKey<Campaign>(c => c.HuntId);
            e.Property(c => c.Status).HasConversion<string>();
            e.Property(c => c.TokenBudget).HasPrecision(18, 2);
        });

        modelBuilder.Entity<FraudAlert>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Status).HasConversion<string>();
        });

        modelBuilder.Entity<MagicLink>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasOne(m => m.User).WithMany().HasForeignKey(m => m.UserId);
            e.HasIndex(m => m.Token).IsUnique();
        });

        modelBuilder.Entity<Listing>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasOne(l => l.Seller).WithMany(u => u.Listings).HasForeignKey(l => l.SellerId);
            e.HasOne(l => l.Item).WithMany(i => i.Listings).HasForeignKey(l => l.ItemId);
            e.Property(l => l.Price).HasPrecision(18, 2);
            e.HasIndex(l => l.Status);
        });

        modelBuilder.Entity<MarketplacePurchase>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.Listing).WithMany().HasForeignKey(p => p.ListingId);
            e.HasOne(p => p.Buyer).WithMany().HasForeignKey(p => p.BuyerId);
            e.Property(p => p.TotalAmount).HasPrecision(18, 2);
            e.HasIndex(p => p.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL AND \"IdempotencyKey\" != ''");
        });

        modelBuilder.Entity<TradeOffer>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasOne(t => t.Initiator).WithMany(u => u.TradeOffersAsInitiator).HasForeignKey(t => t.InitiatorId);
            e.HasOne(t => t.Receiver).WithMany(u => u.TradeOffersAsReceiver).HasForeignKey(t => t.ReceiverId);
            e.HasIndex(t => t.Status);
        });

        modelBuilder.Entity<TradeOfferItem>(e =>
        {
            e.HasKey(ti => ti.Id);
            e.HasOne(ti => ti.TradeOffer).WithMany(t => t.Items).HasForeignKey(ti => ti.TradeOfferId);
            e.HasOne(ti => ti.Item).WithMany().HasForeignKey(ti => ti.ItemId).IsRequired(false);
            e.Property(ti => ti.TokenAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Auction>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Seller).WithMany(u => u.AuctionsAsSeller).HasForeignKey(a => a.SellerId);
            e.HasOne(a => a.Item).WithMany().HasForeignKey(a => a.ItemId);
            e.HasOne(a => a.HighestBid).WithMany().HasForeignKey(a => a.HighestBidId).IsRequired(false);
            e.Property(a => a.ReservePrice).HasPrecision(18, 2);
            e.Property(a => a.MinIncrement).HasPrecision(18, 2);
            e.HasIndex(a => a.Status);
            e.HasIndex(a => a.EndTime);
        });

        modelBuilder.Entity<Bid>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasOne(b => b.Auction).WithMany(a => a.Bids).HasForeignKey(b => b.AuctionId);
            e.HasOne(b => b.Bidder).WithMany(u => u.Bids).HasForeignKey(b => b.BidderId);
            e.Property(b => b.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<CommissionSchema>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Value).HasPrecision(18, 4);
            e.Property(c => c.PlatformShare).HasPrecision(18, 4);
            e.Property(c => c.OrganiserShare).HasPrecision(18, 4);
        });

        modelBuilder.Entity<Payout>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.Organiser).WithMany(u => u.Payouts).HasForeignKey(p => p.OrganiserId);
            e.Property(p => p.Amount).HasPrecision(18, 2);
            e.HasIndex(p => p.Status);
        });

        modelBuilder.Entity<LeaderboardEntry>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasOne(l => l.Player).WithMany(u => u.LeaderboardEntries).HasForeignKey(l => l.PlayerId);
            e.Property(l => l.Score).HasPrecision(18, 2);
            e.HasIndex(l => new { l.Scope, l.Period, l.Metric, l.Rank });
        });

        modelBuilder.Entity<Achievement>(e =>
        {
            e.HasKey(a => a.Id);
        });

        modelBuilder.Entity<PlayerAchievement>(e =>
        {
            e.HasKey(pa => pa.Id);
            e.HasOne(pa => pa.Player).WithMany(u => u.PlayerAchievements).HasForeignKey(pa => pa.PlayerId);
            e.HasOne(pa => pa.Achievement).WithMany(a => a.PlayerAchievements).HasForeignKey(pa => pa.AchievementId);
            e.HasIndex(pa => new { pa.PlayerId, pa.AchievementId }).IsUnique();
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasOne(n => n.User).WithMany(u => u.Notifications).HasForeignKey(n => n.UserId);
            e.HasIndex(n => new { n.UserId, n.IsRead });
        });

        modelBuilder.Entity<NotificationPreference>(e =>
        {
            e.HasKey(np => np.Id);
            e.HasOne(np => np.User).WithMany(u => u.NotificationPreferences).HasForeignKey(np => np.UserId);
            e.HasIndex(np => new { np.UserId, np.Category }).IsUnique();
        });
    }
}
