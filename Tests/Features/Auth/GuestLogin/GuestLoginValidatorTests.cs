using Lootopia.Api.Features.Auth.GuestLogin;

namespace Tests.Features.Auth.GuestLogin;

public class GuestLoginValidatorTests
{
    private readonly GuestLoginValidator _validator = new();

    [Fact]
    public void Validate_WithValidDeviceId_ShouldBeValid()
    {
        // Arrange
        var command = new GuestLoginCommand("device-id-12345");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyDeviceId_ShouldFail(string? deviceId)
    {
        // Arrange
        var command = new GuestLoginCommand(deviceId!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GuestLoginCommand.DeviceId));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_WithDeviceIdTooShort_ShouldFail(string deviceId)
    {
        // Arrange
        var command = new GuestLoginCommand(deviceId);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GuestLoginCommand.DeviceId));
    }
}
