using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Features.Auth.GuestLogin;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests.Features.Auth.GuestLogin;

public class GuestLoginHandlerTests
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Theory, AutoData]
    public async Task Handle_WithNewDeviceId_ShouldCreateUserAndReturnSuccess(
        string accessToken, string refreshToken)
    {
        // Arrange
        const string deviceId = "device-abc-12345";
        await using var db = CreateDbContext();
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        _tokenServiceMock
            .Setup(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()))
            .Returns((accessToken, refreshToken, refreshExpiry));

        var handler = new GuestLoginHandler(db, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new GuestLoginCommand(deviceId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(accessToken, result.Value.AccessToken);
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Once);
        var user = await db.Users.FirstOrDefaultAsync(u => u.DeviceId == deviceId);
        Assert.NotNull(user);
        Assert.True(user.IsGuest);
    }

    [Theory, AutoData]
    public async Task Handle_WithExistingDeviceId_ShouldReturnSuccessWithoutCreatingNewUser(
        string accessToken, string refreshToken)
    {
        // Arrange
        const string deviceId = "existing-device-99";
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            IsGuest = true,
            IsActive = true,
            DisplayName = "Guest_existing"
        };
        await using var db = CreateDbContext();
        db.Users.Add(existingUser);
        await db.SaveChangesAsync();

        _tokenServiceMock
            .Setup(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()))
            .Returns((accessToken, refreshToken, DateTime.UtcNow.AddDays(7)));

        var handler = new GuestLoginHandler(db, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new GuestLoginCommand(deviceId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, db.Users.Count(u => u.DeviceId == deviceId));
        _tokenServiceMock.Verify(s => s.GenerateTokens(It.IsAny<User>(), It.IsAny<string?>()), Times.Once);
    }
}
