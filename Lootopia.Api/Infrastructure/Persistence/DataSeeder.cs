using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Lootopia.Api.Infrastructure.Persistence;

public static class DataSeeder
{
    private static readonly GeometryFactory Geo = new(new PrecisionModel(), 4326);

    // Stable IDs for cross-references
    private static readonly Guid AdminId   = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid PartnerId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid PlayerId  = Guid.Parse("00000000-0000-0000-0000-000000000003");
    private static readonly Guid Player2Id = Guid.Parse("00000000-0000-0000-0000-000000000004");
    private static readonly Guid Player3Id = Guid.Parse("00000000-0000-0000-0000-000000000005");
    private static readonly Guid Player4Id = Guid.Parse("00000000-0000-0000-0000-000000000006");
    private static readonly Guid Player5Id = Guid.Parse("00000000-0000-0000-0000-000000000007");
    private static readonly Guid Partner2Id = Guid.Parse("00000000-0000-0000-0000-000000000008");

    public static async Task SeedAsync(LootopiaDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var adminUser = new User
        {
            Id = AdminId,
            Email = "admin@lootopia.io",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            DisplayName = "Master Admin",
            Role = UserRole.Admin,
            IsGuest = false,
            IsActive = true
        };

        var partnerUser = new User
        {
            Id = PartnerId,
            Email = "partner@lootopia.io",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Partner123!"),
            DisplayName = "Jean-Pierre Boulanger",
            Role = UserRole.Partner,
            IsGuest = false,
            IsActive = true
        };

        var playerUser = new User
        {
            Id = PlayerId,
            Email = "player@lootopia.io",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Player123!"),
            DisplayName = "Lucas le Gamer",
            Role = UserRole.Player,
            IsGuest = false,
            IsActive = true
        };

        context.Users.AddRange(adminUser, partnerUser, playerUser);

        context.Wallets.AddRange(
            new Wallet { Id = Guid.NewGuid(), UserId = adminUser.Id, Balance = 999999 },
            new Wallet { Id = Guid.NewGuid(), UserId = partnerUser.Id, Balance = 5000 },
            new Wallet { Id = Guid.NewGuid(), UserId = playerUser.Id, Balance = 100 }
        );

        var partner = new Partner
        {
            Id = Guid.NewGuid(),
            UserId = partnerUser.Id,
            BusinessName = "Boulangerie Jean-Pierre",
            Address = "12 Rue de la Paix, Lyon",
            TokenBudget = 5000
        };
        context.Partners.Add(partner);

        await context.SaveChangesAsync();
    }

    public static async Task SeedAchievementsAsync(LootopiaDbContext context)
    {
        if (await context.Achievements.AnyAsync())
            return;

        context.Achievements.AddRange(
            Ach("Première Chasse", "Complétez votre première chasse au trésor", "Common", 10, "HuntsCompleted", 1),
            Ach("Explorateur", "Complétez 5 chasses", "Common", 25, "HuntsCompleted", 5),
            Ach("Vétéran", "Complétez 10 chasses", "Rare", 50, "HuntsCompleted", 10),
            Ach("Légende", "Complétez 25 chasses", "Epic", 100, "HuntsCompleted", 25),
            Ach("Speedrunner", "Complétez une chasse en moins de 10 min", "Epic", 100, "SpeedRun", 600),
            Ach("Collectionneur", "Possédez 20 items différents", "Rare", 50, "ItemsOwned", 20),
            Ach("Commerçant", "Vendez 5 items au marketplace", "Common", 25, "ItemsSold", 5),
            Ach("Enchérisseur", "Gagnez 3 enchères", "Rare", 50, "AuctionsWon", 3),
            Ach("Millionnaire", "Accumulez 10 000 LTK", "Legendary", 200, "BalanceReached", 10000),
            Ach("Social", "Effectuez 3 échanges P2P", "Common", 25, "TradesCompleted", 3)
        );
        await context.SaveChangesAsync();
    }

    public static async Task SeedTestDataAsync(LootopiaDbContext context)
    {
        if (await context.Hunts.AnyAsync())
            return;

        // ── Additional players ──
        var players = new[]
        {
            new User { Id = Player2Id, Email = "sophie@lootopia.io", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Player123!"), DisplayName = "Sophie l'Aventurière", Role = UserRole.Player, IsGuest = false, IsActive = true },
            new User { Id = Player3Id, Email = "karim@lootopia.io", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Player123!"), DisplayName = "Karim Explorer", Role = UserRole.Player, IsGuest = false, IsActive = true },
            new User { Id = Player4Id, Email = "emma@lootopia.io", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Player123!"), DisplayName = "Emma la Rapide", Role = UserRole.Player, IsGuest = false, IsActive = true },
            new User { Id = Player5Id, Email = "thomas@lootopia.io", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Player123!"), DisplayName = "Thomas le Rusé", Role = UserRole.Player, IsGuest = false, IsActive = true },
            new User { Id = Partner2Id, Email = "partenaire2@lootopia.io", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Partner123!"), DisplayName = "Marie Café", Role = UserRole.Partner, IsGuest = false, IsActive = true },
        };
        context.Users.AddRange(players);

        var walletP2 = new Wallet { Id = Guid.NewGuid(), UserId = Player2Id, Balance = 2350 };
        var walletP3 = new Wallet { Id = Guid.NewGuid(), UserId = Player3Id, Balance = 780 };
        var walletP4 = new Wallet { Id = Guid.NewGuid(), UserId = Player4Id, Balance = 4200 };
        var walletP5 = new Wallet { Id = Guid.NewGuid(), UserId = Player5Id, Balance = 150 };
        var walletPa2 = new Wallet { Id = Guid.NewGuid(), UserId = Partner2Id, Balance = 8000 };
        context.Wallets.AddRange(walletP2, walletP3, walletP4, walletP5, walletPa2);

        // Update player@lootopia.io wallet balance
        var mainWallet = await context.Wallets.FirstAsync(w => w.UserId == PlayerId);
        mainWallet.Balance = 1500;

        var partner2 = new Partner { Id = Guid.NewGuid(), UserId = Partner2Id, BusinessName = "Café Marie", Address = "45 Avenue des Champs-Élysées, Paris", TokenBudget = 8000 };
        context.Partners.Add(partner2);

        await context.SaveChangesAsync();

        // ── Items catalog ──
        var items = new[]
        {
            MakeItem("Boussole Magique", "Une boussole qui pointe toujours vers le trésor le plus proche", ItemRarity.Common, ItemType.Artifact, true),
            MakeItem("Carte au Trésor Ancienne", "Un parchemin révélant des indices cachés", ItemRarity.Rare, ItemType.Artifact, true),
            MakeItem("Amulette du Chercheur", "Augmente la portée de détection des caches", ItemRarity.Epic, ItemType.Artifact, true),
            MakeItem("Couronne du Roi Pirate", "L'artefact légendaire de Barbe d'Or", ItemRarity.Legendary, ItemType.Artifact, true),
            MakeItem("Coupon Boulangerie -20%", "Réduction chez Jean-Pierre", ItemRarity.Common, ItemType.Coupon, false),
            MakeItem("Coupon Café Gratuit", "Un café offert chez Marie", ItemRarity.Common, ItemType.Coupon, false),
            MakeItem("Coupon Restaurant -30%", "30% de réduction au restaurant Le Gourmet", ItemRarity.Rare, ItemType.Coupon, false),
            MakeItem("Coupon Spa Premium", "Accès VIP au spa de luxe", ItemRarity.Epic, ItemType.Coupon, false),
            MakeItem("Jeton Bonus x2", "Double les récompenses de la prochaine chasse", ItemRarity.Rare, ItemType.Token, true),
            MakeItem("Jeton Indice", "Révèle un indice supplémentaire pendant une chasse", ItemRarity.Common, ItemType.Token, true),
            MakeItem("Jeton Téléportation", "Permet de valider une étape à distance", ItemRarity.Epic, ItemType.Token, true),
            MakeItem("Clé Mystérieuse", "Ouvre l'accès à des chasses secrètes", ItemRarity.Legendary, ItemType.Token, true),
        };
        context.Items.AddRange(items);
        await context.SaveChangesAsync();

        // ── Hunts (Paris area) ──
        var hunt1Id = Guid.NewGuid();
        var hunt2Id = Guid.NewGuid();
        var hunt3Id = Guid.NewGuid();
        var hunt4Id = Guid.NewGuid();
        var hunt5Id = Guid.NewGuid();

        context.Hunts.AddRange(
            new Hunt { Id = hunt1Id, Title = "Le Secret de la Tour Eiffel", Description = "Parcourez le Champ de Mars et découvrez les mystères cachés autour de la Dame de Fer.", Status = HuntStatus.Active, CreatedBy = AdminId, Difficulty = 2, RewardTokens = 150, StartDate = DateTime.UtcNow.AddDays(-30) },
            new Hunt { Id = hunt2Id, Title = "Les Catacombes Perdues", Description = "Explorez les alentours de Denfert-Rochereau pour retrouver les reliques oubliées.", Status = HuntStatus.Active, CreatedBy = AdminId, Difficulty = 4, RewardTokens = 500, StartDate = DateTime.UtcNow.AddDays(-20) },
            new Hunt { Id = hunt3Id, Title = "Trésor du Marais", Description = "Flânez dans le quartier du Marais à la recherche d'un trésor caché dans ses ruelles médiévales.", Status = HuntStatus.Active, CreatedBy = AdminId, Difficulty = 1, RewardTokens = 80, StartDate = DateTime.UtcNow.AddDays(-15) },
            new Hunt { Id = hunt4Id, Title = "L'Énigme de Montmartre", Description = "Résolvez les énigmes de la butte Montmartre, du Sacré-Cœur aux vignes.", Status = HuntStatus.Active, CreatedBy = AdminId, Difficulty = 3, RewardTokens = 300, StartDate = DateTime.UtcNow.AddDays(-10) },
            new Hunt { Id = hunt5Id, Title = "La Quête du Louvre", Description = "Une chasse au trésor épique autour du musée le plus célèbre du monde.", Status = HuntStatus.Draft, CreatedBy = AdminId, Difficulty = 5, RewardTokens = 1000 }
        );
        await context.SaveChangesAsync();

        // ── Hunt Steps ──
        // Hunt 1: Tour Eiffel area
        context.HuntSteps.AddRange(
            Step(hunt1Id, 1, 48.8584, 2.2945, 30, "Trouvez le banc avec la plaque dorée face à la Dame de Fer"),
            Step(hunt1Id, 2, 48.8567, 2.2977, 25, "Cherchez la fontaine aux lions au bout de l'allée"),
            Step(hunt1Id, 3, 48.8530, 2.3012, 20, "Le trésor se cache près du pont le plus romantique de Paris")
        );

        // Hunt 2: Catacombes area
        context.HuntSteps.AddRange(
            Step(hunt2Id, 1, 48.8338, 2.3325, 35, "Face au lion de bronze, tournez vers le sud"),
            Step(hunt2Id, 2, 48.8312, 2.3367, 30, "Le passage secret est marqué d'une croix sur le mur est"),
            Step(hunt2Id, 3, 48.8290, 2.3310, 25, "Descendez 14 marches et cherchez la pierre marquée"),
            Step(hunt2Id, 4, 48.8275, 2.3340, 20, "La relique dort sous l'arbre centenaire du parc")
        );

        // Hunt 3: Le Marais
        context.HuntSteps.AddRange(
            Step(hunt3Id, 1, 48.8566, 2.3615, 30, "Commencez devant la plus ancienne maison de Paris"),
            Step(hunt3Id, 2, 48.8554, 2.3580, 25, "Le falafel le plus célèbre de la rue cache un secret")
        );

        // Hunt 4: Montmartre
        context.HuntSteps.AddRange(
            Step(hunt4Id, 1, 48.8867, 2.3431, 30, "Au pied des marches qui mènent au ciel blanc"),
            Step(hunt4Id, 2, 48.8845, 2.3375, 25, "Le mur où l'amour est écrit en mille langues"),
            Step(hunt4Id, 3, 48.8862, 2.3340, 20, "Les dernières vignes de Paris gardent le trésor")
        );

        // Hunt 5: Louvre (draft)
        context.HuntSteps.AddRange(
            Step(hunt5Id, 1, 48.8606, 2.3376, 30, "La pyramide de verre cache plus qu'il n'y paraît"),
            Step(hunt5Id, 2, 48.8619, 2.3352, 25, "Les arches du Carrousel murmurent des secrets"),
            Step(hunt5Id, 3, 48.8599, 2.3280, 20, "Les jardins royaux cachent le passage final"),
            Step(hunt5Id, 4, 48.8634, 2.3275, 15, "Le pont des Arts porte la clé ultime")
        );
        await context.SaveChangesAsync();

        // ── Player inventories ──
        context.PlayerInventories.AddRange(
            Inv(PlayerId, items[0].Id, 1, AcquisitionSource.Hunt),
            Inv(PlayerId, items[4].Id, 2, AcquisitionSource.Hunt),
            Inv(PlayerId, items[9].Id, 3, AcquisitionSource.Hunt),
            Inv(PlayerId, items[8].Id, 1, AcquisitionSource.Purchase),
            Inv(Player2Id, items[1].Id, 1, AcquisitionSource.Hunt),
            Inv(Player2Id, items[2].Id, 1, AcquisitionSource.Hunt),
            Inv(Player2Id, items[5].Id, 3, AcquisitionSource.Hunt),
            Inv(Player2Id, items[10].Id, 1, AcquisitionSource.Marketplace),
            Inv(Player3Id, items[0].Id, 2, AcquisitionSource.Hunt),
            Inv(Player3Id, items[6].Id, 1, AcquisitionSource.Hunt),
            Inv(Player3Id, items[3].Id, 1, AcquisitionSource.Auction),
            Inv(Player4Id, items[1].Id, 1, AcquisitionSource.Trade),
            Inv(Player4Id, items[7].Id, 1, AcquisitionSource.Hunt),
            Inv(Player4Id, items[11].Id, 1, AcquisitionSource.Hunt),
            Inv(Player4Id, items[9].Id, 5, AcquisitionSource.Hunt),
            Inv(Player5Id, items[0].Id, 1, AcquisitionSource.Hunt),
            Inv(Player5Id, items[4].Id, 1, AcquisitionSource.Hunt)
        );
        await context.SaveChangesAsync();

        // ── Player hunts (completed + in-progress) ──
        var now = DateTime.UtcNow;
        context.PlayerHunts.AddRange(
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = PlayerId, HuntId = hunt1Id, CurrentStepOrder = 3, Status = PlayerHuntStatus.Completed, StartedAt = now.AddDays(-28), CompletedAt = now.AddDays(-28).AddMinutes(45) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = PlayerId, HuntId = hunt3Id, CurrentStepOrder = 2, Status = PlayerHuntStatus.Completed, StartedAt = now.AddDays(-14), CompletedAt = now.AddDays(-14).AddMinutes(20) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = PlayerId, HuntId = hunt4Id, CurrentStepOrder = 1, Status = PlayerHuntStatus.InProgress, StartedAt = now.AddDays(-1) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = Player2Id, HuntId = hunt1Id, CurrentStepOrder = 3, Status = PlayerHuntStatus.Completed, StartedAt = now.AddDays(-25), CompletedAt = now.AddDays(-25).AddMinutes(38) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = Player2Id, HuntId = hunt2Id, CurrentStepOrder = 4, Status = PlayerHuntStatus.Completed, StartedAt = now.AddDays(-18), CompletedAt = now.AddDays(-18).AddHours(1.5) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = Player2Id, HuntId = hunt3Id, CurrentStepOrder = 2, Status = PlayerHuntStatus.Completed, StartedAt = now.AddDays(-12), CompletedAt = now.AddDays(-12).AddMinutes(15) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = Player2Id, HuntId = hunt4Id, CurrentStepOrder = 3, Status = PlayerHuntStatus.Completed, StartedAt = now.AddDays(-5), CompletedAt = now.AddDays(-5).AddMinutes(55) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = Player3Id, HuntId = hunt1Id, CurrentStepOrder = 3, Status = PlayerHuntStatus.Completed, StartedAt = now.AddDays(-22), CompletedAt = now.AddDays(-22).AddMinutes(50) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = Player3Id, HuntId = hunt2Id, CurrentStepOrder = 2, Status = PlayerHuntStatus.InProgress, StartedAt = now.AddDays(-2) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = Player4Id, HuntId = hunt1Id, CurrentStepOrder = 3, Status = PlayerHuntStatus.Completed, StartedAt = now.AddDays(-20), CompletedAt = now.AddDays(-20).AddMinutes(8) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = Player4Id, HuntId = hunt2Id, CurrentStepOrder = 4, Status = PlayerHuntStatus.Completed, StartedAt = now.AddDays(-15), CompletedAt = now.AddDays(-15).AddMinutes(72) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = Player4Id, HuntId = hunt3Id, CurrentStepOrder = 2, Status = PlayerHuntStatus.Completed, StartedAt = now.AddDays(-8), CompletedAt = now.AddDays(-8).AddMinutes(18) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = Player4Id, HuntId = hunt4Id, CurrentStepOrder = 3, Status = PlayerHuntStatus.Completed, StartedAt = now.AddDays(-3), CompletedAt = now.AddDays(-3).AddMinutes(42) },
            new PlayerHunt { Id = Guid.NewGuid(), PlayerId = Player5Id, HuntId = hunt3Id, CurrentStepOrder = 1, Status = PlayerHuntStatus.InProgress, StartedAt = now.AddHours(-3) }
        );
        await context.SaveChangesAsync();

        // ── Wallet transactions ──
        var mainWalletId = mainWallet.Id;
        context.WalletTransactions.AddRange(
            Tx(mainWalletId, 150, TransactionType.Credit, "Récompense: Le Secret de la Tour Eiffel", now.AddDays(-28)),
            Tx(mainWalletId, 80, TransactionType.Credit, "Récompense: Trésor du Marais", now.AddDays(-14)),
            Tx(mainWalletId, -50, TransactionType.Debit, "Achat marketplace: Jeton Bonus x2", now.AddDays(-10)),
            Tx(mainWalletId, -25, TransactionType.Debit, "Commission vente marketplace", now.AddDays(-7)),
            Tx(mainWalletId, 200, TransactionType.Credit, "Vente marketplace: Boussole Magique", now.AddDays(-7)),
            Tx(walletP2.Id, 150, TransactionType.Credit, "Récompense: Le Secret de la Tour Eiffel", now.AddDays(-25)),
            Tx(walletP2.Id, 500, TransactionType.Credit, "Récompense: Les Catacombes Perdues", now.AddDays(-18)),
            Tx(walletP2.Id, 80, TransactionType.Credit, "Récompense: Trésor du Marais", now.AddDays(-12)),
            Tx(walletP2.Id, 300, TransactionType.Credit, "Récompense: L'Énigme de Montmartre", now.AddDays(-5)),
            Tx(walletP2.Id, -100, TransactionType.Debit, "Achat marketplace: Jeton Téléportation", now.AddDays(-4)),
            Tx(walletP4.Id, 150, TransactionType.Credit, "Récompense: Le Secret de la Tour Eiffel", now.AddDays(-20)),
            Tx(walletP4.Id, 500, TransactionType.Credit, "Récompense: Les Catacombes Perdues", now.AddDays(-15)),
            Tx(walletP4.Id, 80, TransactionType.Credit, "Récompense: Trésor du Marais", now.AddDays(-8)),
            Tx(walletP4.Id, 300, TransactionType.Credit, "Récompense: L'Énigme de Montmartre", now.AddDays(-3)),
            Tx(walletP4.Id, 1000, TransactionType.Credit, "Bonus: Speedrunner achievement", now.AddDays(-3))
        );
        await context.SaveChangesAsync();

        // ── Marketplace listings ──
        context.Listings.AddRange(
            new Listing { Id = Guid.NewGuid(), SellerId = Player2Id, ItemId = items[1].Id, Price = 350, Stock = 1, Status = "Active", CreatedAt = now.AddDays(-3) },
            new Listing { Id = Guid.NewGuid(), SellerId = Player3Id, ItemId = items[0].Id, Price = 45, Stock = 1, Status = "Active", CreatedAt = now.AddDays(-2) },
            new Listing { Id = Guid.NewGuid(), SellerId = Player4Id, ItemId = items[9].Id, Price = 15, Stock = 3, Status = "Active", CreatedAt = now.AddDays(-1) },
            new Listing { Id = Guid.NewGuid(), SellerId = PlayerId, ItemId = items[9].Id, Price = 12, Stock = 2, Status = "Active", CreatedAt = now.AddHours(-5) },
            new Listing { Id = Guid.NewGuid(), SellerId = Player4Id, ItemId = items[7].Id, Price = 800, Stock = 1, Status = "Active", CreatedAt = now.AddHours(-2) },
            new Listing { Id = Guid.NewGuid(), SellerId = Player2Id, ItemId = items[5].Id, Price = 25, Stock = 1, Status = "Sold", CreatedAt = now.AddDays(-10) }
        );
        await context.SaveChangesAsync();

        // ── Auctions (insert in stages to avoid circular dependency) ──
        var auction1Id = Guid.NewGuid();
        var auction2Id = Guid.NewGuid();

        context.Auctions.AddRange(
            new Auction { Id = auction1Id, SellerId = Player2Id, ItemId = items[2].Id, ReservePrice = 200, MinIncrement = 50, StartTime = now.AddDays(-1), EndTime = now.AddDays(2), Status = "Active", CreatedAt = now.AddDays(-1) },
            new Auction { Id = auction2Id, SellerId = Player4Id, ItemId = items[11].Id, ReservePrice = 500, MinIncrement = 100, StartTime = now.AddHours(-6), EndTime = now.AddDays(5), Status = "Active", CreatedAt = now.AddHours(-6) }
        );
        await context.SaveChangesAsync();

        var bid1 = new Bid { Id = Guid.NewGuid(), AuctionId = auction1Id, BidderId = PlayerId, Amount = 250, CreatedAt = now.AddHours(-10) };
        var bid2 = new Bid { Id = Guid.NewGuid(), AuctionId = auction1Id, BidderId = Player3Id, Amount = 320, CreatedAt = now.AddHours(-6) };
        var bid3 = new Bid { Id = Guid.NewGuid(), AuctionId = auction1Id, BidderId = Player4Id, Amount = 400, CreatedAt = now.AddHours(-2) };
        context.Bids.AddRange(bid1, bid2, bid3);
        await context.SaveChangesAsync();

        var auction1 = await context.Auctions.FindAsync(auction1Id);
        auction1!.HighestBidId = bid3.Id;
        await context.SaveChangesAsync();

        // ── Campaigns ──
        var partnerEntity = await context.Partners.FirstAsync(p => p.UserId == PartnerId);
        context.Campaigns.AddRange(
            new Campaign { Id = Guid.NewGuid(), PartnerId = partnerEntity.Id, Title = "Promo Boulangerie Printemps", HuntId = hunt1Id, TokenBudget = 2000, CouponsDistributed = 45, MaxCoupons = 100, Status = CampaignStatus.Active, ActivatedAt = now.AddDays(-30), ExpiresAt = now.AddDays(30) },
            new Campaign { Id = Guid.NewGuid(), PartnerId = partnerEntity.Id, Title = "Opération Croissant d'Or", TokenBudget = 1000, CouponsDistributed = 0, MaxCoupons = 50, Status = CampaignStatus.Draft },
            new Campaign { Id = Guid.NewGuid(), PartnerId = partner2.Id, Title = "Café Découverte", HuntId = hunt3Id, TokenBudget = 3000, CouponsDistributed = 23, MaxCoupons = 200, Status = CampaignStatus.Active, ActivatedAt = now.AddDays(-15), ExpiresAt = now.AddDays(45) }
        );
        await context.SaveChangesAsync();

        // ── Leaderboard entries ──
        context.LeaderboardEntries.AddRange(
            LB(Player4Id, "global", "all", "points", 2030, 1),
            LB(Player2Id, "global", "all", "points", 1030, 2),
            LB(PlayerId, "global", "all", "points", 330, 3),
            LB(Player3Id, "global", "all", "points", 150, 4),
            LB(Player5Id, "global", "all", "points", 0, 5),
            LB(Player4Id, "global", "all", "hunts_completed", 4, 1),
            LB(Player2Id, "global", "all", "hunts_completed", 4, 2),
            LB(PlayerId, "global", "all", "hunts_completed", 2, 3),
            LB(Player3Id, "global", "all", "hunts_completed", 1, 4),
            LB(Player4Id, "global", "week", "points", 300, 1),
            LB(Player2Id, "global", "week", "points", 300, 2),
            LB(PlayerId, "global", "week", "points", 0, 3)
        );
        await context.SaveChangesAsync();

        // ── Player achievements ──
        var achievements = await context.Achievements.ToListAsync();
        var firstHunt = achievements.First(a => a.Name == "Première Chasse");
        var explorer = achievements.First(a => a.Name == "Explorateur");
        var speedrunner = achievements.First(a => a.Name == "Speedrunner");

        context.PlayerAchievements.AddRange(
            new PlayerAchievement { Id = Guid.NewGuid(), PlayerId = PlayerId, AchievementId = firstHunt.Id, UnlockedAt = now.AddDays(-28) },
            new PlayerAchievement { Id = Guid.NewGuid(), PlayerId = Player2Id, AchievementId = firstHunt.Id, UnlockedAt = now.AddDays(-25) },
            new PlayerAchievement { Id = Guid.NewGuid(), PlayerId = Player3Id, AchievementId = firstHunt.Id, UnlockedAt = now.AddDays(-22) },
            new PlayerAchievement { Id = Guid.NewGuid(), PlayerId = Player4Id, AchievementId = firstHunt.Id, UnlockedAt = now.AddDays(-20) },
            new PlayerAchievement { Id = Guid.NewGuid(), PlayerId = Player4Id, AchievementId = speedrunner.Id, UnlockedAt = now.AddDays(-20) }
        );
        await context.SaveChangesAsync();

        // ── Fraud alerts ──
        context.FraudAlerts.AddRange(
            new FraudAlert { Id = Guid.NewGuid(), Type = "SpeedAnomaly", Description = "Vitesse de déplacement anormale détectée : 150 km/h entre deux validations en 30 secondes", RelatedUserId = Player5Id, Severity = "High", Status = FraudAlertStatus.New, CreatedAt = now.AddHours(-3) },
            new FraudAlert { Id = Guid.NewGuid(), Type = "RapidValidation", Description = "10 validations en 5 secondes au même point GPS", RelatedUserId = Player5Id, Severity = "Critical", Status = FraudAlertStatus.New, CreatedAt = now.AddHours(-2) },
            new FraudAlert { Id = Guid.NewGuid(), Type = "SuspiciousTrading", Description = "Échanges multiples entre les mêmes comptes en boucle", RelatedUserId = Player3Id, Severity = "Medium", Status = FraudAlertStatus.Acknowledged, CreatedAt = now.AddDays(-5) }
        );
        await context.SaveChangesAsync();

        // ── Commission schemas ──
        context.CommissionSchemas.AddRange(
            new CommissionSchema { Id = Guid.NewGuid(), Type = "Percentage", Value = 0.05m, PlatformShare = 0.6m, OrganiserShare = 0.4m, IsDefault = true },
            new CommissionSchema { Id = Guid.NewGuid(), Type = "Fixed", Value = 10, PlatformShare = 1.0m, OrganiserShare = 0.0m, IsDefault = false }
        );
        await context.SaveChangesAsync();

        // ── Notifications ──
        context.Notifications.AddRange(
            new Notification { Id = Guid.NewGuid(), UserId = PlayerId, Type = "Achievement", Title = "Badge débloqué !", Body = "Vous avez obtenu le badge Première Chasse", IsRead = true, CreatedAt = now.AddDays(-28) },
            new Notification { Id = Guid.NewGuid(), UserId = PlayerId, Type = "Hunt", Title = "Nouvelle chasse disponible", Body = "L'Énigme de Montmartre vous attend !", IsRead = false, CreatedAt = now.AddDays(-10) },
            new Notification { Id = Guid.NewGuid(), UserId = PlayerId, Type = "Marketplace", Title = "Votre article a été vendu", Body = "Boussole Magique vendue pour 200 LTK", IsRead = false, CreatedAt = now.AddDays(-7) },
            new Notification { Id = Guid.NewGuid(), UserId = PlayerId, Type = "Auction", Title = "Vous avez été surenchéri", Body = "Quelqu'un a enchéri 320 LTK sur l'Amulette du Chercheur", IsRead = false, CreatedAt = now.AddHours(-6) },
            new Notification { Id = Guid.NewGuid(), UserId = Player2Id, Type = "Achievement", Title = "Badge débloqué !", Body = "Vous avez obtenu le badge Première Chasse", IsRead = true, CreatedAt = now.AddDays(-25) },
            new Notification { Id = Guid.NewGuid(), UserId = Player4Id, Type = "Achievement", Title = "Badge débloqué !", Body = "Vous avez obtenu le badge Speedrunner", IsRead = true, CreatedAt = now.AddDays(-20) },
            new Notification { Id = Guid.NewGuid(), UserId = Player4Id, Type = "System", Title = "Bienvenue dans le top 1", Body = "Vous êtes maintenant premier du classement général !", IsRead = false, CreatedAt = now.AddDays(-3) }
        );
        await context.SaveChangesAsync();
    }

    // ── Helper methods ──
    private static Achievement Ach(string name, string desc, string rarity, int pts, string rule, int val) => new()
    {
        Id = Guid.NewGuid(), Name = name, Description = desc, Rarity = rarity,
        PointsValue = pts, RuleType = rule, RuleConfig = $"{{\"RequiredCount\":{val}}}"
    };

    private static Item MakeItem(string name, string desc, ItemRarity rarity, ItemType type, bool tradeable) => new()
    {
        Id = Guid.NewGuid(), Name = name, Description = desc, Rarity = rarity,
        Type = type, IsTradeable = tradeable
    };

    private static HuntStep Step(Guid huntId, int order, double lat, double lng, double radius, string clue) => new()
    {
        Id = Guid.NewGuid(), HuntId = huntId, StepOrder = order,
        Location = Geo.CreatePoint(new Coordinate(lng, lat)),
        RadiusMeters = radius, Clue = clue, ActionType = StepActionType.Reach
    };

    private static PlayerInventory Inv(Guid playerId, Guid itemId, int qty, AcquisitionSource src) => new()
    {
        Id = Guid.NewGuid(), PlayerId = playerId, ItemId = itemId,
        Quantity = qty, Source = src, AcquiredAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30))
    };

    private static WalletTransaction Tx(Guid walletId, decimal amount, TransactionType type, string reason, DateTime date) => new()
    {
        Id = Guid.NewGuid(), WalletId = walletId, Amount = Math.Abs(amount),
        Type = type, Reason = reason, CreatedAt = date
    };

    private static LeaderboardEntry LB(Guid playerId, string scope, string period, string metric, decimal score, int rank) => new()
    {
        Id = Guid.NewGuid(), PlayerId = playerId, Scope = scope,
        Period = period, Metric = metric, Score = score, Rank = rank, CalculatedAt = DateTime.UtcNow
    };
}
