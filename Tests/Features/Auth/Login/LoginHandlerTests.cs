using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Features.Auth.Login;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests.Features.Auth.Login;

public class LoginHandlerTests
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Theory, AutoData]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessWithTokens(
        string accessToken, string refreshToken)
    {
        // Arrange
        const string password = "password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "valid@test.com",
            PasswordHash = passwordHash,
            IsGuest = false,
            IsActive = true
        };
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        await using var db = CreateDbContext();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        _tokenServiceMock
            .Setup(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()))
            .Returns((accessToken, refreshToken, refreshExpiry));

        var handler = new LoginHandler(db, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new LoginCommand("valid@test.com", password), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(accessToken, result.Value.AccessToken);
        Assert.Equal(refreshToken, result.Value.RefreshToken);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new LoginHandler(db, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new LoginCommand("nobody@test.com", "password"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "wrong@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            IsGuest = false,
            IsActive = true
        };
        await using var db = CreateDbContext();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new LoginHandler(db, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new LoginCommand("wrong@test.com", "wrongpassword"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDisabledAccount_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "disabled@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsGuest = false,
            IsActive = false
        };
        await using var db = CreateDbContext();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new LoginHandler(db, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new LoginCommand("disabled@test.com", "password123"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.AccountDisabled", result.Error.Code);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithGuestUser_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "guest@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsGuest = true,
            IsActive = true
        };
        await using var db = CreateDbContext();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new LoginHandler(db, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new LoginCommand("guest@test.com", "password123"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Never);
    }
}
