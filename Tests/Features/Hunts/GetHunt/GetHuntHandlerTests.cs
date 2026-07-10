using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Hunts.GetHunt;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Tests.Features.Hunts.GetHunt;

public class GetHuntValidatorTests
{
    private readonly GetHuntValidator _validator = new();

    [Fact]
    public void Validate_WithValidHuntId_ShouldBeValid()
    {
        var result = _validator.Validate(new GetHuntQuery(Guid.NewGuid()));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyHuntId_ShouldFail()
    {
        var result = _validator.Validate(new GetHuntQuery(Guid.Empty));
        Assert.False(result.IsValid);
    }
}

public class GetHuntHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Theory, AutoData]
    public async Task Handle_WithExistingHunt_ShouldReturnHunt(Guid huntId)
    {
        // Arrange
        await using var db = CreateDbContext();
        var hunt = new Hunt
        {
            Id = huntId,
            Title = "My Hunt",
            Description = "Desc",
            Status = HuntStatus.Active,
            Difficulty = 2,
            RewardTokens = 50,
            CreatedBy = Guid.NewGuid()
        };
        db.Hunts.Add(hunt);
        await db.SaveChangesAsync();

        var handler = new GetHuntHandler(db);

        // Act
        var result = await handler.Handle(new GetHuntQuery(huntId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(huntId, result.Value.Id);
        Assert.Equal("My Hunt", result.Value.Title);
    }

    [Fact]
    public async Task Handle_WithNonExistentHunt_ShouldReturnNotFound()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new GetHuntHandler(db);

        // Act
        var result = await handler.Handle(new GetHuntQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }
}
