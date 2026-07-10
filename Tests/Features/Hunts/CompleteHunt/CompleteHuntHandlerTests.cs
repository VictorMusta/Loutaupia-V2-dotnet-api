using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Hunts.CompleteHunt;
using Lootopia.Api.Features.Hunts.GetHunt;
using Lootopia.Api.Features.Hunts.GetMyHunts;
using Lootopia.Api.Features.Hunts.ListAllHunts;
using Lootopia.Api.Features.Hunts.StartHunt;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;

namespace Tests.Features.Hunts.CompleteHunt;

public class CompleteHuntHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_WithValidPlayerHunt_ShouldMarkCompleted()
    {
        // Arrange
        await using var db = CreateDbContext();
        var playerId = Guid.NewGuid();
        var hunt = new Hunt { Id = Guid.NewGuid(), Title = "Test", Status = HuntStatus.Active, CreatedBy = Guid.NewGuid(), RewardTokens = 0 };
        var playerHunt = new PlayerHunt
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            HuntId = hunt.Id,
            Status = PlayerHuntStatus.InProgress,
            CurrentStepOrder = 1
        };

        db.Hunts.Add(hunt);
        db.PlayerHunts.Add(playerHunt);
        await db.SaveChangesAsync();

        var walletServiceMock = new Mock<IWalletService>();
        var handler = new CompleteHuntHandler(db, walletServiceMock.Object);

        // Act
        var result = await handler.Handle(new CompleteHuntCommand(playerHunt.Id), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updated = await db.PlayerHunts.FindAsync(playerHunt.Id);
        Assert.Equal(PlayerHuntStatus.Completed, updated!.Status);
        Assert.NotNull(updated.CompletedAt);
    }

    [Fact]
    public async Task Handle_WithNonExistentPlayerHunt_ShouldReturnNotFound()
    {
        // Arrange
        await using var db = CreateDbContext();
        var walletServiceMock = new Mock<IWalletService>();
        var handler = new CompleteHuntHandler(db, walletServiceMock.Object);

        // Act
        var result = await handler.Handle(new CompleteHuntCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Handle_WithReward_ShouldCreditWallet()
    {
        // Arrange
        await using var db = CreateDbContext();
        var playerId = Guid.NewGuid();
        var hunt = new Hunt { Id = Guid.NewGuid(), Title = "Test", Status = HuntStatus.Active, CreatedBy = Guid.NewGuid(), RewardTokens = 100 };
        var playerHunt = new PlayerHunt
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            HuntId = hunt.Id,
            Status = PlayerHuntStatus.InProgress,
            CurrentStepOrder = 1
        };

        db.Hunts.Add(hunt);
        db.PlayerHunts.Add(playerHunt);
        await db.SaveChangesAsync();

        var walletServiceMock = new Mock<IWalletService>();
        walletServiceMock
            .Setup(w => w.CreditAsync(playerId, 100m, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new CompleteHuntHandler(db, walletServiceMock.Object);

        // Act
        var result = await handler.Handle(new CompleteHuntCommand(playerHunt.Id), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        walletServiceMock.Verify(w => w.CreditAsync(playerId, 100m, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
