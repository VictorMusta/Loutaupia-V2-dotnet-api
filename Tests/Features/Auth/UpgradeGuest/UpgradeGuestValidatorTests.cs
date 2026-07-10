using Lootopia.Api.Features.Auth.UpgradeGuest;

namespace Tests.Features.Auth.UpgradeGuest;

public class UpgradeGuestValidatorTests
{
    private readonly UpgradeGuestValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = new UpgradeGuestCommand(Guid.NewGuid(), "user@example.com", "Password1!", "John Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var command = new UpgradeGuestCommand(Guid.Empty, "user@example.com", "Password1!", "John");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpgradeGuestCommand.UserId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_ShouldFail(string email)
    {
        // Arrange
        var command = new UpgradeGuestCommand(Guid.NewGuid(), email, "Password1!", "John");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpgradeGuestCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Validate_WithInvalidPassword_ShouldFail(string password)
    {
        // Arrange
        var command = new UpgradeGuestCommand(Guid.NewGuid(), "user@example.com", password, "John");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpgradeGuestCommand.Password));
    }

    [Fact]
    public void Validate_WithDisplayNameExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var command = new UpgradeGuestCommand(Guid.NewGuid(), "user@example.com", "Password1!", new string('x', 101));

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpgradeGuestCommand.DisplayName));
    }
}
