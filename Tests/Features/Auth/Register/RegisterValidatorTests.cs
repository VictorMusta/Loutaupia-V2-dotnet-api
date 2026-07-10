using Lootopia.Api.Features.Auth.Register;

namespace Tests.Features.Auth.Register;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", "Password1!", "John Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_ShouldFail(string email)
    {
        // Arrange
        var command = new RegisterCommand(email, "Password1!", "John");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Validate_WithInvalidPassword_ShouldFail(string password)
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", password, "John");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.Password));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyDisplayName_ShouldFail(string? displayName)
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", "Password1!", displayName!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.DisplayName));
    }

    [Fact]
    public void Validate_WithDisplayNameExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", "Password1!", new string('a', 101));

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.DisplayName));
    }
}
