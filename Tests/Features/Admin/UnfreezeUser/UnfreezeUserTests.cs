using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Features.Admin.UnfreezeUser;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Features.Admin.UnfreezeUser;

public class UnfreezeUserValidatorTests
{
    private readonly UnfreezeUserValidator _validator = new();

    [Fact]
    public void Validate_WithValidUserId_ShouldBeValid()
    {
        // Arrange
        var command = new UnfreezeUserCommand(Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var command = new UnfreezeUserCommand(Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UnfreezeUserCommand.UserId));
    }
}

public class UnfreezeUserHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Theory, AutoData]
    public async Task Handle_WithExistingUser_ShouldActivateUser(Guid userId)
    {
        // Arrange
        var user = new User { Id = userId, IsActive = false };
        await using var db = CreateDbContext();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new UnfreezeUserHandler(db);

        // Act
        var result = await handler.Handle(new UnfreezeUserCommand(userId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updated = await db.Users.FindAsync(userId);
        Assert.NotNull(updated);
        Assert.True(updated.IsActive);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new UnfreezeUserHandler(db);

        // Act
        var result = await handler.Handle(new UnfreezeUserCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.NotFound", result.Error.Code);
    }
}
