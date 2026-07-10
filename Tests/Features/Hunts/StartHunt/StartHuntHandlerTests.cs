using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Hunts.StartHunt;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Tests.Features.Hunts.StartHunt;

public class StartHuntHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static HuntStep CreateStep(Guid huntId) => new()
    {
        Id = Guid.NewGuid(),
        HuntId = huntId,
        StepOrder = 1,
        Location = new GeometryFactory(new PrecisionModel(), 4326).CreatePoint(new Coordinate(2.35, 48.85)),
        RadiusMeters = 50,
        Clue = "Look around"
    };

    [Fact]
    public async Task Handle_WithValidActiveHunt_ShouldCreatePlayerHunt()
    {
        // Arrange
        await using var db = CreateDbContext();
        var playerId = Guid.NewGuid();
        var huntId = Guid.NewGuid();
        var user = new User { Id = playerId, DisplayName = "Player" };
        var hunt = new Hunt { Id = huntId, Title = "Hunt", Status = HuntStatus.Active, CreatedBy = Guid.NewGuid() };
        var step = CreateStep(huntId);

        db.Users.Add(user);
        db.Hunts.Add(hunt);
        db.HuntSteps.Add(step);
        await db.SaveChangesAsync();

        var handler = new StartHuntHandler(db);

        // Act
        var result = await handler.Handle(new StartHuntCommand(playerId, huntId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Look around", result.Value.Clue);
        Assert.Equal(1, result.Value.StepOrder);
    }

    [Fact]
    public async Task Handle_WithNonExistentPlayer_ShouldReturnError()
    {
        // Arrange
        await using var db = CreateDbContext();
        var huntId = Guid.NewGuid();
        var hunt = new Hunt { Id = huntId, Title = "Hunt", Status = HuntStatus.Active, CreatedBy = Guid.NewGuid() };
        db.Hunts.Add(hunt);
        await db.SaveChangesAsync();

        var handler = new StartHuntHandler(db);

        // Act
        var result = await handler.Handle(new StartHuntCommand(Guid.NewGuid(), huntId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.UserNotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithInactiveHunt_ShouldReturnError()
    {
        // Arrange
        await using var db = CreateDbContext();
        var playerId = Guid.NewGuid();
        var huntId = Guid.NewGuid();
        var user = new User { Id = playerId, DisplayName = "Player" };
        var hunt = new Hunt { Id = huntId, Title = "Hunt", Status = HuntStatus.Draft, CreatedBy = Guid.NewGuid() };

        db.Users.Add(user);
        db.Hunts.Add(hunt);
        await db.SaveChangesAsync();

        var handler = new StartHuntHandler(db);

        // Act
        var result = await handler.Handle(new StartHuntCommand(playerId, huntId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Hunt.NotActive", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenAlreadyInProgress_ShouldReturnCurrentStep()
    {
        // Arrange
        await using var db = CreateDbContext();
        var playerId = Guid.NewGuid();
        var huntId = Guid.NewGuid();
        var user = new User { Id = playerId, DisplayName = "Player" };
        var hunt = new Hunt { Id = huntId, Title = "Hunt", Status = HuntStatus.Active, CreatedBy = Guid.NewGuid() };
        var step = CreateStep(huntId);
        var playerHunt = new PlayerHunt
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            HuntId = huntId,
            Status = PlayerHuntStatus.InProgress,
            CurrentStepOrder = 1
        };

        db.Users.Add(user);
        db.Hunts.Add(hunt);
        db.HuntSteps.Add(step);
        db.PlayerHunts.Add(playerHunt);
        await db.SaveChangesAsync();

        var handler = new StartHuntHandler(db);

        // Act
        var result = await handler.Handle(new StartHuntCommand(playerId, huntId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.StepOrder);
    }
}
