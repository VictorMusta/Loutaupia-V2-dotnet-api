using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Features.Auctions.ListAuctions;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Features.Auctions.ListAuctions;

public class ListAuctionsValidatorTests
{
    private readonly ListAuctionsValidator _validator = new();

    [Fact]
    public void Validate_WithValidParams_ShouldBeValid()
    {
        var result = _validator.Validate(new ListAuctionsQuery(null, 1, 10));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithZeroPage_ShouldFail()
    {
        var result = _validator.Validate(new ListAuctionsQuery(null, 0, 10));
        Assert.False(result.IsValid);
    }
}

public class ListAuctionsHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_WithNoAuctions_ShouldReturnEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new ListAuctionsHandler(db);

        // Act
        var result = await handler.Handle(new ListAuctionsQuery(null, 1, 10), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_WithActiveAuctions_ShouldReturnThem()
    {
        // Arrange
        await using var db = CreateDbContext();
        var itemId = Guid.NewGuid();
        db.Items.Add(new Item { Id = itemId, Name = "Sword" });
        db.Auctions.Add(new Auction
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            ItemId = itemId,
            ReservePrice = 10m,
            EndTime = DateTime.UtcNow.AddHours(1),
            Status = "Active"
        });
        await db.SaveChangesAsync();

        var handler = new ListAuctionsHandler(db);

        // Act
        var result = await handler.Handle(new ListAuctionsQuery(null, 1, 10), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldFilterByStatus()
    {
        // Arrange
        await using var db = CreateDbContext();
        var itemId = Guid.NewGuid();
        db.Items.Add(new Item { Id = itemId, Name = "Sword" });
        db.Auctions.Add(new Auction { Id = Guid.NewGuid(), SellerId = Guid.NewGuid(), ItemId = itemId, ReservePrice = 10m, EndTime = DateTime.UtcNow.AddHours(1), Status = "Active" });
        db.Auctions.Add(new Auction { Id = Guid.NewGuid(), SellerId = Guid.NewGuid(), ItemId = itemId, ReservePrice = 10m, EndTime = DateTime.UtcNow.AddHours(-1), Status = "Closed" });
        await db.SaveChangesAsync();

        var handler = new ListAuctionsHandler(db);

        // Act
        var result = await handler.Handle(new ListAuctionsQuery("Closed", 1, 10), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
    }
}
