using Lootopia.Api.Domain.Enums;
using Lootopia.Api.Features.Hunts.CreateHunt;

namespace Tests.Features.Hunts.CreateHunt;

public class CreateHuntValidatorTests
{
    private readonly CreateHuntValidator _validator = new();

    private static CreateHuntCommand ValidCommand() => new(
        CreatedBy: Guid.NewGuid(),
        Title: "Test Hunt",
        Description: "A valid description",
        Difficulty: 3,
        RewardTokens: 100m,
        MaxWinners: 5,
        RewardItemId: null,
        PartnerId: null,
        Steps: [new CreateHuntStepDto(48.8566, 2.3522, 50, "Go to this spot", StepActionType.Reach)]
    );

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = ValidCommand();

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyCreatedBy_ShouldFail()
    {
        // Arrange
        var command = ValidCommand() with { CreatedBy = Guid.Empty };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHuntCommand.CreatedBy));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyTitle_ShouldFail(string? title)
    {
        // Arrange
        var command = ValidCommand() with { Title = title! };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHuntCommand.Title));
    }

    [Fact]
    public void Validate_WithTitleExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var command = ValidCommand() with { Title = new string('a', 201) };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHuntCommand.Title));
    }

    [Fact]
    public void Validate_WithDescriptionExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var command = ValidCommand() with { Description = new string('x', 2001) };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHuntCommand.Description));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Validate_WithInvalidDifficulty_ShouldFail(int difficulty)
    {
        // Arrange
        var command = ValidCommand() with { Difficulty = difficulty };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHuntCommand.Difficulty));
    }

    [Fact]
    public void Validate_WithNegativeRewardTokens_ShouldFail()
    {
        // Arrange
        var command = ValidCommand() with { RewardTokens = -1m };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHuntCommand.RewardTokens));
    }

    [Fact]
    public void Validate_WithZeroMaxWinners_ShouldFail()
    {
        // Arrange
        var command = ValidCommand() with { MaxWinners = 0 };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHuntCommand.MaxWinners));
    }

    [Fact]
    public void Validate_WithEmptySteps_ShouldFail()
    {
        // Arrange
        var command = ValidCommand() with { Steps = [] };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHuntCommand.Steps));
    }

    [Theory]
    [InlineData(-91, 2.3)]
    [InlineData(91, 2.3)]
    public void Validate_WithInvalidStepLatitude_ShouldFail(double latitude, double longitude)
    {
        // Arrange
        var command = ValidCommand() with
        {
            Steps = [new CreateHuntStepDto(latitude, longitude, 50, "Clue", StepActionType.Reach)]
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(48.8, -181)]
    [InlineData(48.8, 181)]
    public void Validate_WithInvalidStepLongitude_ShouldFail(double latitude, double longitude)
    {
        // Arrange
        var command = ValidCommand() with
        {
            Steps = [new CreateHuntStepDto(latitude, longitude, 50, "Clue", StepActionType.Reach)]
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(501)]
    public void Validate_WithInvalidStepRadius_ShouldFail(double radius)
    {
        // Arrange
        var command = ValidCommand() with
        {
            Steps = [new CreateHuntStepDto(48.8, 2.3, radius, "Clue", StepActionType.Reach)]
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyStepClue_ShouldFail()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Steps = [new CreateHuntStepDto(48.8, 2.3, 50, "", StepActionType.Reach)]
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
    }
}
