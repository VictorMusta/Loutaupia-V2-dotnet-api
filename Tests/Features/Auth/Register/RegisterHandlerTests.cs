using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Features.Auth.Register;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests.Features.Auth.Register;

public class RegisterHandlerTests
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Theory, AutoData]
    public async Task Handle_WithNewEmail_ShouldReturnSuccessWithTokens(
        string accessToken, string refreshToken)
    {
        // Arrange
        await using var db = CreateDbContext();
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        _tokenServiceMock
            .Setup(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()))
            .Returns((accessToken, refreshToken, refreshExpiry));

        var handler = new RegisterHandler(db, _tokenServiceMock.Object);
        var command = new RegisterCommand("newuser@test.com", "Password1!", "New User");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(accessToken, result.Value.AccessToken);
        Assert.Equal(refreshToken, result.Value.RefreshToken);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@test.com",
            IsGuest = false
        };
        await using var db = CreateDbContext();
        db.Users.Add(existingUser);
        await db.SaveChangesAsync();

        var handler = new RegisterHandler(db, _tokenServiceMock.Object);
        var command = new RegisterCommand("existing@test.com", "Password1!", "Someone");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.EmailInUse", result.Error.Code);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Never);
    }

    [Theory, AutoData]
    public async Task Handle_WithNewEmail_ShouldPersistUserAndWallet(string displayName)
    {
        // Arrange
        await using var db = CreateDbContext();
        _tokenServiceMock
            .Setup(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()))
            .Returns(("at", "rt", DateTime.UtcNow.AddDays(7)));

        var handler = new RegisterHandler(db, _tokenServiceMock.Object);
        var command = new RegisterCommand("persist@test.com", "Password1!", displayName);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "persist@test.com");
        Assert.NotNull(user);
        Assert.False(user.IsGuest);
        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id);
        Assert.NotNull(wallet);
        Assert.Equal(0, wallet.Balance);
    }
}
