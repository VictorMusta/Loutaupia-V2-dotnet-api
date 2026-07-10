using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Admin.FreezeCampaign;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Features.Admin.FreezeCampaign;

public class FreezeCampaignValidatorTests
{
    private readonly FreezeCampaignValidator _validator = new();

    [Fact]
    public void Validate_WithValidCampaignId_ShouldBeValid()
    {
        // Arrange
        var command = new FreezeCampaignCommand(Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyCampaignId_ShouldFail()
    {
        // Arrange
        var command = new FreezeCampaignCommand(Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(FreezeCampaignCommand.CampaignId));
    }
}

public class FreezeCampaignHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Theory, AutoData]
    public async Task Handle_WithExistingCampaign_ShouldFreezeCampaign(Guid campaignId)
    {
        // Arrange
        var partner = new Partner { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), BusinessName = "Partner" };
        var campaign = new Campaign { Id = campaignId, PartnerId = partner.Id, Status = CampaignStatus.Active };
        await using var db = CreateDbContext();
        db.Partners.Add(partner);
        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync();

        var handler = new FreezeCampaignHandler(db);

        // Act
        var result = await handler.Handle(new FreezeCampaignCommand(campaignId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updated = await db.Campaigns.FindAsync(campaignId);
        Assert.NotNull(updated);
        Assert.Equal(CampaignStatus.Frozen, updated.Status);
    }

    [Fact]
    public async Task Handle_WithNonExistentCampaign_ShouldReturnFailure()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new FreezeCampaignHandler(db);

        // Act
        var result = await handler.Handle(new FreezeCampaignCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Campaign.NotFound", result.Error.Code);
    }
}
