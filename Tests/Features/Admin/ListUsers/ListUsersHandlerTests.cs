using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Admin.ListUsers;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Features.Admin.ListUsers;

public class ListUsersHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_WithNoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new ListUsersHandler(db);

        // Act
        var result = await handler.Handle(new ListUsersQuery(1, 10, null), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.Total);
    }

    [Fact]
    public async Task Handle_WithUsers_ShouldReturnAll()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Users.Add(new User { Id = Guid.NewGuid(), DisplayName = "Alice", Email = "alice@test.com", Role = UserRole.Player });
        db.Users.Add(new User { Id = Guid.NewGuid(), DisplayName = "Bob", Email = "bob@test.com", Role = UserRole.Player });
        await db.SaveChangesAsync();

        var handler = new ListUsersHandler(db);

        // Act
        var result = await handler.Handle(new ListUsersQuery(1, 10, null), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Total);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldFilterResults()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.Users.Add(new User { Id = Guid.NewGuid(), DisplayName = "Alice", Email = "alice@test.com" });
        db.Users.Add(new User { Id = Guid.NewGuid(), DisplayName = "Bob", Email = "bob@test.com" });
        await db.SaveChangesAsync();

        var handler = new ListUsersHandler(db);

        // Act
        var result = await handler.Handle(new ListUsersQuery(1, 10, "alice"), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("Alice", result.Value.Items[0].DisplayName);
    }

    [Fact]
    public async Task Handle_ShouldRespectPagination()
    {
        // Arrange
        await using var db = CreateDbContext();
        for (var i = 0; i < 5; i++)
            db.Users.Add(new User { Id = Guid.NewGuid(), DisplayName = $"User{i}", Email = $"u{i}@test.com" });
        await db.SaveChangesAsync();

        var handler = new ListUsersHandler(db);

        // Act
        var result = await handler.Handle(new ListUsersQuery(1, 2, null), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(5, result.Value.Total);
    }
}
