using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Admin.GetActivityReport;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Features.Admin.GetActivityReport;

public class GetAdminReportHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_WithEmptyDb_ShouldReturnZeroCounts()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new GetAdminReportHandler(db);
        var query = new GetAdminReportQuery(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.TotalUsers);
        Assert.Equal(0, result.Value.ActiveHunts);
        Assert.Equal(0, result.Value.PendingAlerts);
    }

    [Fact]
    public async Task Handle_WithUsersAndHunts_ShouldReturnCorrectCounts()
    {
        // Arrange
        await using var db = CreateDbContext();

        var user = new User { Id = Guid.NewGuid(), DisplayName = "Test", CreatedAt = DateTime.UtcNow };
        var activeHunt = new Hunt { Id = Guid.NewGuid(), Status = HuntStatus.Active, Title = "Active", CreatedBy = Guid.NewGuid() };
        var alert = new FraudAlert { Id = Guid.NewGuid(), Status = FraudAlertStatus.New, Type = "Test", Description = "Test" };

        db.Users.Add(user);
        db.Hunts.Add(activeHunt);
        db.FraudAlerts.Add(alert);
        await db.SaveChangesAsync();

        var handler = new GetAdminReportHandler(db);
        var query = new GetAdminReportQuery(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(1));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalUsers);
        Assert.Equal(1, result.Value.ActiveHunts);
        Assert.Equal(1, result.Value.PendingAlerts);
    }

    [Fact]
    public async Task Handle_WithRegistrationsInRange_ShouldGroupByDay()
    {
        // Arrange
        await using var db = CreateDbContext();

        var from = DateTime.UtcNow.AddDays(-3);
        var to = DateTime.UtcNow.AddDays(1);

        db.Users.Add(new User { Id = Guid.NewGuid(), DisplayName = "A", CreatedAt = DateTime.UtcNow });
        db.Users.Add(new User { Id = Guid.NewGuid(), DisplayName = "B", CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var handler = new GetAdminReportHandler(db);
        var query = new GetAdminReportQuery(from, to);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.RegistrationsPerDay);
    }
}
