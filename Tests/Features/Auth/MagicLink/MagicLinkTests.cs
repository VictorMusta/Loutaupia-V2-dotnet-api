using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Auth.MagicLink;
using Lootopia.Api.Infrastructure.Persistence;
using Lootopia.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests.Features.Auth.MagicLink;

public class GenerateMagicLinkValidatorTests
{
    private readonly GenerateMagicLinkValidator _validator = new();

    [Fact]
    public void Validate_WithValidIds_ShouldBeValid()
    {
        var result = _validator.Validate(new GenerateMagicLinkCommand(Guid.NewGuid(), Guid.NewGuid()));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyPartnerUserId_ShouldFail()
    {
        var result = _validator.Validate(new GenerateMagicLinkCommand(Guid.Empty, Guid.NewGuid()));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GenerateMagicLinkCommand.PartnerUserId));
    }

    [Fact]
    public void Validate_WithEmptyAdminId_ShouldFail()
    {
        var result = _validator.Validate(new GenerateMagicLinkCommand(Guid.NewGuid(), Guid.Empty));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GenerateMagicLinkCommand.RequestingAdminId));
    }
}

public class ValidateMagicLinkValidatorTests
{
    private readonly ValidateMagicLinkValidator _validator = new();

    [Fact]
    public void Validate_WithValidToken_ShouldBeValid()
    {
        var result = _validator.Validate(new ValidateMagicLinkCommand("valid-token"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyToken_ShouldFail()
    {
        var result = _validator.Validate(new ValidateMagicLinkCommand(string.Empty));
        Assert.False(result.IsValid);
    }
}

public class GenerateMagicLinkHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_WhenAdminRequestsForPartner_ShouldGenerateLink()
    {
        // Arrange
        await using var db = CreateDbContext();
        var adminId = Guid.NewGuid();
        var partnerId = Guid.NewGuid();

        db.Users.Add(new User { Id = adminId, DisplayName = "Admin", Role = UserRole.Admin });
        db.Users.Add(new User { Id = partnerId, DisplayName = "Partner", Role = UserRole.Partner });
        await db.SaveChangesAsync();

        var handler = new GenerateMagicLinkHandler(db);

        // Act
        var result = await handler.Handle(new GenerateMagicLinkCommand(partnerId, adminId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Token);
        Assert.Contains("token=", result.Value.MagicLinkUrl);
    }

    [Fact]
    public async Task Handle_WhenRequesterIsNotAdmin_ShouldReturnForbidden()
    {
        // Arrange
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        db.Users.Add(new User { Id = userId, DisplayName = "Player", Role = UserRole.Player });
        await db.SaveChangesAsync();

        var handler = new GenerateMagicLinkHandler(db);

        // Act
        var result = await handler.Handle(new GenerateMagicLinkCommand(Guid.NewGuid(), userId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Handle_WhenPartnerNotFound_ShouldReturnNotFound()
    {
        // Arrange
        await using var db = CreateDbContext();
        var adminId = Guid.NewGuid();
        db.Users.Add(new User { Id = adminId, DisplayName = "Admin", Role = UserRole.Admin });
        await db.SaveChangesAsync();

        var handler = new GenerateMagicLinkHandler(db);

        // Act
        var result = await handler.Handle(new GenerateMagicLinkCommand(Guid.NewGuid(), adminId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Handle_WhenTargetUserIsNotPartner_ShouldReturnError()
    {
        // Arrange
        await using var db = CreateDbContext();
        var adminId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        db.Users.Add(new User { Id = adminId, DisplayName = "Admin", Role = UserRole.Admin });
        db.Users.Add(new User { Id = playerId, DisplayName = "Player", Role = UserRole.Player });
        await db.SaveChangesAsync();

        var handler = new GenerateMagicLinkHandler(db);

        // Act
        var result = await handler.Handle(new GenerateMagicLinkCommand(playerId, adminId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("MagicLink.InvalidUser", result.Error.Code);
    }
}

public class ValidateMagicLinkHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_WithValidToken_ShouldReturnTokens()
    {
        // Arrange
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, DisplayName = "Partner", Role = UserRole.Partner, IsActive = true };
        var magicLink = new Lootopia.Api.Domain.Entities.MagicLink
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "valid-token",
            IsConsumed = false,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        db.Users.Add(user);
        db.MagicLinks.Add(magicLink);
        await db.SaveChangesAsync();

        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock
            .Setup(ts => ts.GenerateTokens(It.IsAny<User>(), null))
            .Returns(("access-token", "refresh-token", DateTime.UtcNow.AddDays(7)));

        var handler = new ValidateMagicLinkHandler(db, tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new ValidateMagicLinkCommand("valid-token"), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value.AccessToken);
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldReturnError()
    {
        // Arrange
        await using var db = CreateDbContext();
        var tokenServiceMock = new Mock<ITokenService>();
        var handler = new ValidateMagicLinkHandler(db, tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new ValidateMagicLinkCommand("non-existent-token"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("MagicLink.Invalid", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithAlreadyUsedToken_ShouldReturnError()
    {
        // Arrange
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, DisplayName = "Partner", IsActive = true };
        var magicLink = new Lootopia.Api.Domain.Entities.MagicLink
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "used-token",
            IsConsumed = true,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        db.Users.Add(user);
        db.MagicLinks.Add(magicLink);
        await db.SaveChangesAsync();

        var tokenServiceMock = new Mock<ITokenService>();
        var handler = new ValidateMagicLinkHandler(db, tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new ValidateMagicLinkCommand("used-token"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("MagicLink.AlreadyUsed", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldReturnError()
    {
        // Arrange
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, DisplayName = "Partner", IsActive = true };
        var magicLink = new Lootopia.Api.Domain.Entities.MagicLink
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "expired-token",
            IsConsumed = false,
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };
        db.Users.Add(user);
        db.MagicLinks.Add(magicLink);
        await db.SaveChangesAsync();

        var tokenServiceMock = new Mock<ITokenService>();
        var handler = new ValidateMagicLinkHandler(db, tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new ValidateMagicLinkCommand("expired-token"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("MagicLink.Expired", result.Error.Code);
    }
}
