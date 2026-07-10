using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Hunts.CreateHunt;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Features.Hunts.CreateHunt;

public class CreateHuntHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CreateHuntCommand ValidCommand(string title = "Test Hunt") => new(
        CreatedBy: Guid.NewGuid(),
        Title: title,
        Description: "Description",
        Difficulty: 3,
        RewardTokens: 50m,
        MaxWinners: 10,
        RewardItemId: null,
        PartnerId: null,
        Steps: [new CreateHuntStepDto(48.8566, 2.3522, 100, "Look around", StepActionType.Reach)]
    );

    [Theory, AutoData]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessWithHuntId(string title)
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new CreateHuntHandler(db);

        // Act
        var result = await handler.Handle(ValidCommand(title), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.HuntId);
    }

    [Theory, AutoData]
    public async Task Handle_WithValidCommand_ShouldPersistHuntAndSteps(string title)
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new CreateHuntHandler(db);

        // Act
        var result = await handler.Handle(ValidCommand(title), CancellationToken.None);

        // Assert
        var hunt = await db.Hunts.FindAsync(result.Value.HuntId);
        Assert.NotNull(hunt);
        Assert.Equal(title, hunt.Title);
        Assert.Equal(HuntStatus.Draft, hunt.Status);
        var steps = db.HuntSteps.Where(s => s.HuntId == result.Value.HuntId).ToList();
        Assert.Single(steps);
    }

    [Fact]
    public async Task Handle_WithValidPartnerId_ShouldCreateCampaign()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var partner = new Lootopia.Api.Domain.Entities.Partner
        {
            Id = partnerId,
            UserId = Guid.NewGuid(),
            BusinessName = "Test Partner"
        };
        await using var db = CreateDbContext();
        db.Partners.Add(partner);
        await db.SaveChangesAsync();

        var command = ValidCommand() with { PartnerId = partnerId };
        var handler = new CreateHuntHandler(db);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var campaign = db.Campaigns.FirstOrDefault(c => c.HuntId == result.Value.HuntId);
        Assert.NotNull(campaign);
        Assert.Equal(CampaignStatus.Active, campaign.Status);
    }
}
