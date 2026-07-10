using AutoFixture.Xunit2;
using Lootopia.Api.Domain.Entities;
using Lootopia.Api.Features.Admin.CreditPartnerBudget;
using Lootopia.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Features.Admin.CreditPartnerBudget;

public class CreditPartnerBudgetValidatorTests
{
    private readonly CreditPartnerBudgetValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = new CreditPartnerBudgetCommand(Guid.NewGuid(), 100m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyPartnerId_ShouldFail()
    {
        // Arrange
        var command = new CreditPartnerBudgetCommand(Guid.Empty, 100m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreditPartnerBudgetCommand.PartnerId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_WithNonPositiveAmount_ShouldFail(decimal amount)
    {
        // Arrange
        var command = new CreditPartnerBudgetCommand(Guid.NewGuid(), amount);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreditPartnerBudgetCommand.Amount));
    }
}

public class CreditPartnerBudgetHandlerTests
{
    private static LootopiaDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<LootopiaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Theory, AutoData]
    public async Task Handle_WithExistingPartner_ShouldCreditBudget(Guid partnerId)
    {
        // Arrange
        var partner = new Partner { Id = partnerId, UserId = Guid.NewGuid(), BusinessName = "Acme", TokenBudget = 500m };
        await using var db = CreateDbContext();
        db.Partners.Add(partner);
        await db.SaveChangesAsync();

        var handler = new CreditPartnerBudgetHandler(db);

        // Act
        var result = await handler.Handle(new CreditPartnerBudgetCommand(partnerId, 200m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updated = await db.Partners.FindAsync(partnerId);
        Assert.NotNull(updated);
        Assert.Equal(700m, updated.TokenBudget);
    }

    [Fact]
    public async Task Handle_WithNonExistentPartner_ShouldReturnFailure()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new CreditPartnerBudgetHandler(db);

        // Act
        var result = await handler.Handle(new CreditPartnerBudgetCommand(Guid.NewGuid(), 100m), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Partner.NotFound", result.Error.Code);
    }
}
