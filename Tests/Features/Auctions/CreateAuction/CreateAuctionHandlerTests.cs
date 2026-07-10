using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Auctions.CreateAuction;
using Lootopia.Api.Features.Auctions.GetAuction;
using Lootopia.Api.Features.Auctions.ListAuctions;
using Lootopia.Api.Features.Auctions.PlaceBid;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Tests.Features.Auctions.CreateAuction;

public class CreateAuctionValidatorTests
{
    private readonly CreateAuctionValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        var result = _validator.Validate(new CreateAuctionCommand(Guid.NewGuid(), Guid.NewGuid(), 10m, 1m, 60));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithNegativeReservePrice_ShouldFail()
    {
        var result = _validator.Validate(new CreateAuctionCommand(Guid.NewGuid(), Guid.NewGuid(), -1m, 1m, 60));
        Assert.False(result.IsValid);
    }
}

public class CreateAuctionHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateAuction()
    {
        // Arrange
        await using var db = CreateDbContext();
        var sellerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var item = new Item { Id = itemId, Name = "Sword", IsTradeable = true };
        var inventory = new PlayerInventory { Id = Guid.NewGuid(), PlayerId = sellerId, ItemId = itemId, Quantity = 1 };

        db.Items.Add(item);
        db.PlayerInventories.Add(inventory);
        await db.SaveChangesAsync();

        var handler = new CreateAuctionHandler(db);

        // Act
        var result = await handler.Handle(
            new CreateAuctionCommand(sellerId, itemId, 10m, 1m, 60),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.AuctionId);
    }

    [Fact]
    public async Task Handle_WithNonExistentItem_ShouldReturnError()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new CreateAuctionHandler(db);

        // Act
        var result = await handler.Handle(
            new CreateAuctionCommand(Guid.NewGuid(), Guid.NewGuid(), 10m, 1m, 60),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auction.ItemNotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithNonTradeableItem_ShouldReturnError()
    {
        // Arrange
        await using var db = CreateDbContext();
        var itemId = Guid.NewGuid();
        var item = new Item { Id = itemId, Name = "Sacred", IsTradeable = false };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var handler = new CreateAuctionHandler(db);

        // Act
        var result = await handler.Handle(
            new CreateAuctionCommand(Guid.NewGuid(), itemId, 10m, 1m, 60),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auction.ItemNotTradeable", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenSellerDoesNotOwnItem_ShouldReturnError()
    {
        // Arrange
        await using var db = CreateDbContext();
        var itemId = Guid.NewGuid();
        var item = new Item { Id = itemId, Name = "Sword", IsTradeable = true };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var handler = new CreateAuctionHandler(db);

        // Act
        var result = await handler.Handle(
            new CreateAuctionCommand(Guid.NewGuid(), itemId, 10m, 1m, 60),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auction.ItemNotOwned", result.Error.Code);
    }
}
