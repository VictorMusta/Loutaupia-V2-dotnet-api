using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Hunts.GetMyHunts;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Tests.Features.Hunts.GetMyHunts;

public class GetMyHuntsHandlerTests
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
        Clue = "A clue"
    };

    [Fact]
    public async Task Handle_WithNoPlayerHunts_ShouldReturnEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new GetMyHuntsHandler(db);

        // Act
        var result = await handler.Handle(new GetMyHuntsQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Hunts);
    }

    [Fact]
    public async Task Handle_WithPlayerHunts_ShouldReturnHunts()
    {
        // Arrange
        await using var db = CreateDbContext();
        var playerId = Guid.NewGuid();
        var huntId = Guid.NewGuid();
        var hunt = new Hunt { Id = huntId, Title = "My Hunt", Status = HuntStatus.Active, CreatedBy = Guid.NewGuid() };
        var step = CreateStep(huntId);
        var playerHunt = new PlayerHunt
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            HuntId = huntId,
            Status = PlayerHuntStatus.InProgress,
            CurrentStepOrder = 1,
            StartedAt = DateTime.UtcNow
        };

        db.Hunts.Add(hunt);
        db.HuntSteps.Add(step);
        db.PlayerHunts.Add(playerHunt);
        await db.SaveChangesAsync();

        var handler = new GetMyHuntsHandler(db);

        // Act
        var result = await handler.Handle(new GetMyHuntsQuery(playerId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Hunts);
        Assert.Equal(huntId, result.Value.Hunts[0].HuntId);
    }
}
