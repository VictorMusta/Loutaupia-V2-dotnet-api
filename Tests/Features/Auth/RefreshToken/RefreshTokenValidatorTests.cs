using Lootopia.Api.Features.Auth.RefreshToken;

namespace Tests.Features.Auth.RefreshToken;

public class RefreshTokenValidatorTests
{
    private readonly RefreshTokenValidator _validator = new();

    [Fact]
    public void Validate_WithValidToken_ShouldBeValid()
    {
        // Arrange
        var command = new RefreshTokenCommand("some-valid-refresh-token");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyToken_ShouldFail(string? token)
    {
        // Arrange
        var command = new RefreshTokenCommand(token!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RefreshTokenCommand.RefreshToken));
    }
}
