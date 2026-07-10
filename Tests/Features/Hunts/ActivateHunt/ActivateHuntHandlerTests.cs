using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Hunts.ActivateHunt;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Tests.Features.Hunts.ActivateHunt;

public class ActivateHuntHandlerTests
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

    [Theory, AutoData]
    public async Task Handle_WithDraftHuntWithSteps_ShouldActivateHunt(Guid huntId)
    {
        // Arrange
        var hunt = new Hunt { Id = huntId, Status = HuntStatus.Draft, Title = "Hunt", CreatedBy = Guid.NewGuid() };
        var step = CreateStep(huntId);
        await using var db = CreateDbContext();
        db.Hunts.Add(hunt);
        db.HuntSteps.Add(step);
        await db.SaveChangesAsync();

        var handler = new ActivateHuntHandler(db);

        // Act
        var result = await handler.Handle(new ActivateHuntCommand(huntId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updated = await db.Hunts.FindAsync(huntId);
        Assert.NotNull(updated);
        Assert.Equal(HuntStatus.Active, updated.Status);
    }

    [Fact]
    public async Task Handle_WithNonExistentHunt_ShouldReturnNotFound()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new ActivateHuntHandler(db);

        // Act
        var result = await handler.Handle(new ActivateHuntCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("General.NotFound", result.Error.Code);
    }

    [Theory, AutoData]
    public async Task Handle_WithNonDraftHunt_ShouldReturnFailure(Guid huntId)
    {
        // Arrange
        var hunt = new Hunt { Id = huntId, Status = HuntStatus.Active, Title = "Hunt", CreatedBy = Guid.NewGuid() };
        var step = CreateStep(huntId);
        await using var db = CreateDbContext();
        db.Hunts.Add(hunt);
        db.HuntSteps.Add(step);
        await db.SaveChangesAsync();

        var handler = new ActivateHuntHandler(db);

        // Act
        var result = await handler.Handle(new ActivateHuntCommand(huntId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Hunt.NotDraft", result.Error.Code);
    }

    [Theory, AutoData]
    public async Task Handle_WithDraftHuntWithoutSteps_ShouldReturnFailure(Guid huntId)
    {
        // Arrange
        var hunt = new Hunt { Id = huntId, Status = HuntStatus.Draft, Title = "Hunt", CreatedBy = Guid.NewGuid() };
        await using var db = CreateDbContext();
        db.Hunts.Add(hunt);
        await db.SaveChangesAsync();

        var handler = new ActivateHuntHandler(db);

        // Act
        var result = await handler.Handle(new ActivateHuntCommand(huntId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Hunt.NoSteps", result.Error.Code);
    }
}
