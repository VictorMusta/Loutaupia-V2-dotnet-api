using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Lootopia.Api.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(LootopiaDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var adminUser = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Email = "admin@lootopia.io",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            DisplayName = "Master Admin",
            Role = UserRole.Admin,
            IsGuest = false
        };

        var partnerUser = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Email = "partner@lootopia.io",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Partner123!"),
            DisplayName = "Jean-Pierre Boulanger",
            Role = UserRole.Partner,
            IsGuest = false
        };

        var playerUser = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Email = "player@lootopia.io",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Player123!"),
            DisplayName = "Lucas le Gamer",
            Role = UserRole.Player,
            IsGuest = false
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

        var achievements = new[]
        {
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "First Hunt",
                Description = "Complete your first hunt",
                Rarity = "Common",
                PointsValue = 10,
                RuleType = "HuntsCompleted",
                RuleConfig = "{\"RequiredCount\":1}"
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Explorer",
                Description = "Complete 5 hunts",
                Rarity = "Common",
                PointsValue = 25,
                RuleType = "HuntsCompleted",
                RuleConfig = "{\"RequiredCount\":5}"
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Veteran",
                Description = "Complete 10 hunts",
                Rarity = "Rare",
                PointsValue = 50,
                RuleType = "HuntsCompleted",
                RuleConfig = "{\"RequiredCount\":10}"
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Speedrunner",
                Description = "Complete a hunt in under 10 minutes",
                Rarity = "Epic",
                PointsValue = 100,
                RuleType = "SpeedRun",
                RuleConfig = "{\"MaxSeconds\":600}"
            }
        };

        context.Achievements.AddRange(achievements);
        await context.SaveChangesAsync();
    }
}
