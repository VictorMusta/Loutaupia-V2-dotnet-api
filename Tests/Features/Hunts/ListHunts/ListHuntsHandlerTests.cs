using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Hunts.ListHunts;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.SharedKernel.Geo;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;

namespace Tests.Features.Hunts.ListHunts;

public class ListHuntsValidatorTests
{
    private readonly ListHuntsValidator _validator = new();

    [Fact]
    public void Validate_WithValidParams_ShouldBeValid()
    {
        var result = _validator.Validate(new ListHuntsQuery(48.85, 2.35, 5));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithInvalidLat_ShouldFail()
    {
        var result = _validator.Validate(new ListHuntsQuery(200, 2.35, 5));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithInvalidLng_ShouldFail()
    {
        var result = _validator.Validate(new ListHuntsQuery(48.85, 300, 5));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithZeroRadius_ShouldFail()
    {
        var result = _validator.Validate(new ListHuntsQuery(48.85, 2.35, 0));
        Assert.False(result.IsValid);
    }
}

public class ListHuntsHandlerTests
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
        Clue = "Look"
    };

    [Fact]
    public async Task Handle_WithNoActiveHunts_ShouldReturnEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var geoMock = new Mock<IGeoValidator>();
        var handler = new ListHuntsHandler(db, geoMock.Object);

        // Act
        var result = await handler.Handle(new ListHuntsQuery(48.85, 2.35, 50), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Hunts);
    }

    [Fact]
    public async Task Handle_WithActiveHuntWithStep_ShouldReturnHunt()
    {
        // Arrange
        await using var db = CreateDbContext();
        var huntId = Guid.NewGuid();
        var hunt = new Hunt { Id = huntId, Title = "Near Hunt", Status = HuntStatus.Active, CreatedBy = Guid.NewGuid() };
        var step = CreateStep(huntId);

        db.Hunts.Add(hunt);
        db.HuntSteps.Add(step);
        await db.SaveChangesAsync();

        var geoMock = new Mock<IGeoValidator>();
        geoMock.Setup(g => g.CalculateDistanceInMeters(It.IsAny<Point>(), It.IsAny<Point>())).Returns(100);

        var handler = new ListHuntsHandler(db, geoMock.Object);

        // Act
        var result = await handler.Handle(new ListHuntsQuery(48.85, 2.35, 50), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Hunts);
    }
}
