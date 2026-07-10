using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Hunts.ListAllHunts;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Features.Hunts.ListAllHunts;

public class ListAllHuntsHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_WithNoHunts_ShouldReturnEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new ListAllHuntsHandler(db);

        // Act
        var result = await handler.Handle(new ListAllHuntsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Hunts);
    }

    [Fact]
    public async Task Handle_WithHunts_ShouldReturnAll()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Hunts.Add(new Hunt { Id = Guid.NewGuid(), Title = "Hunt A", Status = HuntStatus.Active, CreatedBy = Guid.NewGuid() });
        db.Hunts.Add(new Hunt { Id = Guid.NewGuid(), Title = "Hunt B", Status = HuntStatus.Draft, CreatedBy = Guid.NewGuid() });
        await db.SaveChangesAsync();

        var handler = new ListAllHuntsHandler(db);

        // Act
        var result = await handler.Handle(new ListAllHuntsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Hunts.Count);
    }
}
