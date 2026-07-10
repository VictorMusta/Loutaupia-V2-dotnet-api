using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Features.Auth.RefreshToken;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests.Features.Auth.RefreshToken;

public class RefreshTokenHandlerTests
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Theory, AutoData]
    public async Task Handle_WithValidToken_ShouldReturnNewTokens(
        string newAccessToken, string newRefreshToken)
    {
        // Arrange
        const string existingToken = "valid-refresh-token";
        var user = new User
        {
            Id = Guid.NewGuid(),
            RefreshToken = existingToken,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };
        await using var db = CreateDbContext();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var newExpiry = DateTime.UtcNow.AddDays(7);
        _tokenServiceMock
            .Setup(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()))
            .Returns((newAccessToken, newRefreshToken, newExpiry));

        var handler = new RefreshTokenHandler(db, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new RefreshTokenCommand(existingToken), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newAccessToken, result.Value.AccessToken);
        Assert.Equal(newRefreshToken, result.Value.RefreshToken);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldReturnFailure()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new RefreshTokenHandler(db, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new RefreshTokenCommand("invalid-token"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidRefreshToken", result.Error.Code);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            RefreshToken = "expired-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        };
        await using var db = CreateDbContext();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new RefreshTokenHandler(db, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new RefreshTokenCommand("expired-token"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidRefreshToken", result.Error.Code);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDisabledAccount_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            RefreshToken = "active-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
            IsActive = false
        };
        await using var db = CreateDbContext();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new RefreshTokenHandler(db, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new RefreshTokenCommand("active-token"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.AccountDisabled", result.Error.Code);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Never);
    }
}
