using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Hunts.ArchiveHunt;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Features.Hunts.ArchiveHunt;

public class ArchiveHuntHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Theory, AutoData]
    public async Task Handle_WithExistingHunt_ShouldFreezeHunt(Guid huntId)
    {
        // Arrange
        var hunt = new Hunt { Id = huntId, Status = HuntStatus.Active, Title = "Hunt", CreatedBy = Guid.NewGuid() };
        await using var db = CreateDbContext();
        db.Hunts.Add(hunt);
        await db.SaveChangesAsync();

        var handler = new ArchiveHuntHandler(db);

        // Act
        var result = await handler.Handle(new ArchiveHuntCommand(huntId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updated = await db.Hunts.FindAsync(huntId);
        Assert.NotNull(updated);
        Assert.Equal(HuntStatus.Frozen, updated.Status);
    }

    [Fact]
    public async Task Handle_WithNonExistentHunt_ShouldReturnNotFound()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new ArchiveHuntHandler(db);

        // Act
        var result = await handler.Handle(new ArchiveHuntCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("General.NotFound", result.Error.Code);
    }
}
