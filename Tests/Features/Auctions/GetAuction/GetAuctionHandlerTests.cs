using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Features.Auctions.GetAuction;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Features.Auctions.GetAuction;

public class GetAuctionValidatorTests
{
    private readonly GetAuctionValidator _validator = new();

    [Fact]
    public void Validate_WithValidId_ShouldBeValid()
    {
        var result = _validator.Validate(new GetAuctionQuery(Guid.NewGuid()));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldFail()
    {
        var result = _validator.Validate(new GetAuctionQuery(Guid.Empty));
        Assert.False(result.IsValid);
    }
}

public class GetAuctionHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Theory, AutoData]
    public async Task Handle_WithExistingAuction_ShouldReturnAuction(Guid auctionId)
    {
        // Arrange
        await using var db = CreateDbContext();
        var itemId = Guid.NewGuid();
        var item = new Item { Id = itemId, Name = "Sword" };
        var auction = new Auction
        {
            Id = auctionId,
            SellerId = Guid.NewGuid(),
            ItemId = itemId,
            ReservePrice = 10m,
            MinIncrement = 1m,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            Status = "Active"
        };

        db.Items.Add(item);
        db.Auctions.Add(auction);
        await db.SaveChangesAsync();

        var handler = new GetAuctionHandler(db);

        // Act
        var result = await handler.Handle(new GetAuctionQuery(auctionId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(auctionId, result.Value.Id);
        Assert.Equal("Sword", result.Value.ItemName);
    }

    [Fact]
    public async Task Handle_WithNonExistentAuction_ShouldReturnNotFound()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new GetAuctionHandler(db);

        // Act
        var result = await handler.Handle(new GetAuctionQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }
}
