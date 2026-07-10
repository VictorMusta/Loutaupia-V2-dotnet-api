using AutoFixture.Xunit2;
using Lootopia.Api.Features.Auth.Login;

namespace Tests.Features.Auth.Login;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "password123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public void Validate_WithInvalidEmail_ShouldFail(string email)
    {
        // Arrange
        var command = new LoginCommand(email, "password123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void Validate_WithEmptyPassword_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Password));
    }

    [Theory, AutoData]
    public void Validate_WithRandomValidEmail_ShouldBeValid(string localPart)
    {
        // Arrange
        var safeLocal = localPart.Replace("@", "a").Replace(" ", "a");
        var command = new LoginCommand($"{safeLocal}@example.com", "password!");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}
