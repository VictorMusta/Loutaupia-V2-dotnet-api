using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Features.Auth.UpgradeGuest;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests.Features.Auth.UpgradeGuest;

public class UpgradeGuestHandlerTests
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Theory, AutoData]
    public async Task Handle_WithValidGuestUser_ShouldUpgradeAndReturnSuccess(
        string accessToken, string refreshToken)
    {
        // Arrange
        var guestId = Guid.NewGuid();
        var guest = new User { Id = guestId, IsGuest = true, IsActive = true };
        await using var db = CreateDbContext();
        db.Users.Add(guest);
        await db.SaveChangesAsync();

        _tokenServiceMock
            .Setup(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()))
            .Returns((accessToken, refreshToken, DateTime.UtcNow.AddDays(7)));

        var handler = new UpgradeGuestHandler(db, _tokenServiceMock.Object);
        var command = new UpgradeGuestCommand(guestId, "upgraded@test.com", "Password1!", "Upgraded User");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(accessToken, result.Value.AccessToken);
        var updatedUser = await db.Users.FindAsync(guestId);
        Assert.NotNull(updatedUser);
        Assert.False(updatedUser.IsGuest);
        Assert.Equal("upgraded@test.com", updatedUser.Email);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new UpgradeGuestHandler(db, _tokenServiceMock.Object);
        var command = new UpgradeGuestCommand(Guid.NewGuid(), "user@test.com", "Password1!", "Name");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("General.NotFound", result.Error.Code);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAlreadyRegisteredUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var registeredUser = new User { Id = userId, IsGuest = false };
        await using var db = CreateDbContext();
        db.Users.Add(registeredUser);
        await db.SaveChangesAsync();

        var handler = new UpgradeGuestHandler(db, _tokenServiceMock.Object);
        var command = new UpgradeGuestCommand(userId, "user@test.com", "Password1!", "Name");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.NotGuest", result.Error.Code);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmailAlreadyTaken_ShouldReturnFailure()
    {
        // Arrange
        var guestId = Guid.NewGuid();
        var guest = new User { Id = guestId, IsGuest = true };
        var otherUser = new User { Id = Guid.NewGuid(), Email = "taken@test.com", IsGuest = false };
        await using var db = CreateDbContext();
        db.Users.AddRange(guest, otherUser);
        await db.SaveChangesAsync();

        var handler = new UpgradeGuestHandler(db, _tokenServiceMock.Object);
        var command = new UpgradeGuestCommand(guestId, "taken@test.com", "Password1!", "Name");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.EmailInUse", result.Error.Code);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Never);
    }
}
