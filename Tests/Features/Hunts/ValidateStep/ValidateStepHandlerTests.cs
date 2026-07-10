using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Hunts.ValidateStep;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Lootopia.Api.SharedKernel.Geo;
using Lootopia.Api.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;

namespace Tests.Features.Hunts.ValidateStep;

public class ValidateStepValidatorTests
{
    private readonly ValidateStepValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        var result = _validator.Validate(new ValidateStepCommand(Guid.NewGuid(), Guid.NewGuid(), 1, 48.85, 2.35));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyPlayerId_ShouldFail()
    {
        var result = _validator.Validate(new ValidateStepCommand(Guid.Empty, Guid.NewGuid(), 1, 48.85, 2.35));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithInvalidStepOrder_ShouldFail()
    {
        var result = _validator.Validate(new ValidateStepCommand(Guid.NewGuid(), Guid.NewGuid(), 0, 48.85, 2.35));
        Assert.False(result.IsValid);
    }
}

public class ValidateStepHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static HuntStep CreateStep(Guid huntId, int order = 1) => new()
    {
        Id = Guid.NewGuid(),
        HuntId = huntId,
        StepOrder = order,
        Location = new GeometryFactory(new PrecisionModel(), 4326).CreatePoint(new Coordinate(2.35, 48.85)),
        RadiusMeters = 50,
        Clue = "Clue"
    };

    [Fact]
    public async Task Handle_WithNoActivePlayerHunt_ShouldReturnError()
    {
        // Arrange
        await using var db = CreateDbContext();
        var geoMock = new Mock<IGeoValidator>();
        var walletMock = new Mock<IWalletService>();
        var fraudMock = new Mock<IFraudDetector>();
        var handler = new ValidateStepHandler(db, geoMock.Object, walletMock.Object, fraudMock.Object);

        // Act
        var result = await handler.Handle(
            new ValidateStepCommand(Guid.NewGuid(), Guid.NewGuid(), 1, 48.85, 2.35),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Hunt.NotFoundOrNotActive", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenTooFar_ShouldReturnTooFarError()
    {
        // Arrange
        await using var db = CreateDbContext();
        var playerId = Guid.NewGuid();
        var huntId = Guid.NewGuid();
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

        db.Hunts.Add(hunt);
        db.HuntSteps.Add(step);
        db.PlayerHunts.Add(playerHunt);
        await db.SaveChangesAsync();

        var geoMock = new Mock<IGeoValidator>();
        geoMock.Setup(g => g.IsWithinRadius(It.IsAny<Point>(), It.IsAny<Point>(), It.IsAny<double>())).Returns(false);

        var walletMock = new Mock<IWalletService>();
        var fraudMock = new Mock<IFraudDetector>();
        fraudMock.Setup(f => f.CheckForAnomaliesAsync(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new ValidateStepHandler(db, geoMock.Object, walletMock.Object, fraudMock.Object);

        // Act
        var result = await handler.Handle(
            new ValidateStepCommand(playerId, huntId, 1, 48.85, 2.35),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Hunt.TooFar", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithWrongStepOrder_ShouldReturnError()
    {
        // Arrange
        await using var db = CreateDbContext();
        var playerId = Guid.NewGuid();
        var huntId = Guid.NewGuid();
        var hunt = new Hunt { Id = huntId, Title = "Hunt", Status = HuntStatus.Active, CreatedBy = Guid.NewGuid() };
        var step = CreateStep(huntId, 1);
        var playerHunt = new PlayerHunt
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            HuntId = huntId,
            Status = PlayerHuntStatus.InProgress,
            CurrentStepOrder = 1
        };

        db.Hunts.Add(hunt);
        db.HuntSteps.Add(step);
        db.PlayerHunts.Add(playerHunt);
        await db.SaveChangesAsync();

        var geoMock = new Mock<IGeoValidator>();
        var walletMock = new Mock<IWalletService>();
        var fraudMock = new Mock<IFraudDetector>();
        var handler = new ValidateStepHandler(db, geoMock.Object, walletMock.Object, fraudMock.Object);

        // Act - send wrong step order 2 instead of 1
        var result = await handler.Handle(
            new ValidateStepCommand(playerId, huntId, 2, 48.85, 2.35),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Hunt.InvalidStepOrder", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithValidStepAndSingleStep_ShouldCompleteHunt()
    {
        // Arrange
        await using var db = CreateDbContext();
        var playerId = Guid.NewGuid();
        var huntId = Guid.NewGuid();
        var hunt = new Hunt { Id = huntId, Title = "Hunt", Status = HuntStatus.Active, CreatedBy = Guid.NewGuid(), RewardTokens = 0, MaxWinners = 100 };
        var step = CreateStep(huntId, 1);
        var playerHunt = new PlayerHunt
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            HuntId = huntId,
            Status = PlayerHuntStatus.InProgress,
            CurrentStepOrder = 1
        };

        db.Hunts.Add(hunt);
        db.HuntSteps.Add(step);
        db.PlayerHunts.Add(playerHunt);
        await db.SaveChangesAsync();

        var geoMock = new Mock<IGeoValidator>();
        geoMock.Setup(g => g.IsWithinRadius(It.IsAny<Point>(), It.IsAny<Point>(), It.IsAny<double>())).Returns(true);

        var walletMock = new Mock<IWalletService>();
        var fraudMock = new Mock<IFraudDetector>();
        fraudMock.Setup(f => f.CheckForAnomaliesAsync(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new ValidateStepHandler(db, geoMock.Object, walletMock.Object, fraudMock.Object);

        // Act
        var result = await handler.Handle(
            new ValidateStepCommand(playerId, huntId, 1, 48.85, 2.35),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HuntCompleted);
    }
}
