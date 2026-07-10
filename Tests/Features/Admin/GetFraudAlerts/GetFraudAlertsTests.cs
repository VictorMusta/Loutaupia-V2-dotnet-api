using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Admin.GetFraudAlerts;
using Lootopia.Api.Features.Admin.ListUsers;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Features.Admin.GetFraudAlerts;

public class GetFraudAlertsValidatorTests
{
    private readonly GetFraudAlertsValidator _validator = new();

    [Fact]
    public void Validate_WithValidParams_ShouldBeValid()
    {
        var result = _validator.Validate(new GetFraudAlertsQuery(1, 10));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithZeroPage_ShouldFail()
    {
        var result = _validator.Validate(new GetFraudAlertsQuery(0, 10));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithZeroSize_ShouldFail()
    {
        var result = _validator.Validate(new GetFraudAlertsQuery(1, 0));
        Assert.False(result.IsValid);
    }
}

public class GetFraudAlertsHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_WithNoAlerts_ShouldReturnEmptyList()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new GetFraudAlertsHandler(db);

        // Act
        var result = await handler.Handle(new GetFraudAlertsQuery(1, 10), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.Total);
    }

    [Fact]
    public async Task Handle_WithAlerts_ShouldReturnAlerts()
    {
        // Arrange
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, DisplayName = "Suspect", Email = "suspect@test.com" };
        var alert = new FraudAlert
        {
            Id = Guid.NewGuid(),
            Type = "Suspicious",
            Description = "Too fast",
            RelatedUserId = userId,
            Severity = "High",
            Status = FraudAlertStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        db.FraudAlerts.Add(alert);
        await db.SaveChangesAsync();

        var handler = new GetFraudAlertsHandler(db);

        // Act
        var result = await handler.Handle(new GetFraudAlertsQuery(1, 10), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(1, result.Value.Total);
        Assert.Equal("Suspect", result.Value.Items[0].UserName);
    }

    [Fact]
    public async Task Handle_ShouldRespectPagination()
    {
        // Arrange
        await using var db = CreateDbContext();

        for (var i = 0; i < 5; i++)
        {
            db.FraudAlerts.Add(new FraudAlert
            {
                Id = Guid.NewGuid(),
                Type = "T",
                Description = "D",
                Status = FraudAlertStatus.New,
                CreatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();

        var handler = new GetFraudAlertsHandler(db);

        // Act
        var result = await handler.Handle(new GetFraudAlertsQuery(1, 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(5, result.Value.Total);
    }
}
