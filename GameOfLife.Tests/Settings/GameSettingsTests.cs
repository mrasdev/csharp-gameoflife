using GameOfLife.Models;

namespace GameOfLife.Tests.Core;

public class GameSettingsTests : IDisposable
{
    private readonly string _tempFilePath;

    public GameSettingsTests()
    {
        // Creates a unique temporary file path for testing JSON writes
        _tempFilePath = Path.GetTempFileName();
    }

    public void Dispose()
    {
        // Cleanup: Ensures the temporary file is deleted after each test
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }

    // ==========================================
    // 1. PROPERTY CLAMPING TESTS
    // ==========================================

    [Theory]
    [InlineData(-0.5, 0.0)] // Below minimum -> clamped to 0.0
    [InlineData(1.5, 1.0)]  // Above maximum -> clamped to 1.0
    [InlineData(0.4, 0.4)]  // Within bounds  -> unchanged
    public void Density_ShouldBeClampedCorrectly(double input, double expected)
    {
        // Arrange & Act
        var settings = new GameSettings { Density = input };

        // Assert
        Assert.Equal(expected, settings.Density);
    }

    [Theory]
    [InlineData(-10, 1)]  // Negative FPS -> clamped to minimum (1)
    [InlineData(0, 1)]    // Zero FPS     -> clamped to minimum (1)
    [InlineData(30, 30)]  // Valid FPS    -> unchanged
    public void FpsRate_ShouldBeClampedToMinimumOfOne(int input, int expected)
    {
        // Arrange & Act
        var settings = new GameSettings { FpsRate = input };

        // Assert
        Assert.Equal(expected, settings.FpsRate);
    }

    // ==========================================
    // 2. JSON LOADING TESTS (FALLBACK LOGIC)
    // ==========================================

    [Fact]
    public void LoadFromJson_FileNotFound_ShouldReturnDefaultSettings()
    {
        // Arrange
        // Create a path to a file that guaranteed does not exist
        string nonExistentPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        var expectedDefaults = new GameSettings();

        // Act
        GameSettings actualSettings = GameSettings.LoadFromJson(nonExistentPath);

        // Assert
        Assert.NotNull(actualSettings);
        Assert.Equal(expectedDefaults.Width, actualSettings.Width);
        Assert.Equal(expectedDefaults.Height, actualSettings.Height);
        Assert.Equal(expectedDefaults.Density, actualSettings.Density);
        Assert.Equal(expectedDefaults.FpsRate, actualSettings.FpsRate);
    }

    [Fact]
    public void LoadFromJson_CorruptJsonSyntax_ShouldReturnDefaultSettings()
    {
        // Arrange
        // Write completely malformed JSON syntax into the temp file
        string corruptJsonContent = "{ \"Width\": 120, \"Height\": ";
        File.WriteAllText(_tempFilePath, corruptJsonContent);

        var expectedDefaults = new GameSettings();

        // Act
        GameSettings actualSettings = GameSettings.LoadFromJson(_tempFilePath);

        // Assert
        Assert.NotNull(actualSettings);
        // Verify it fell back to defaults instead of applying the partial/corrupt data
        Assert.Equal(expectedDefaults.Width, actualSettings.Width);
        Assert.Equal(expectedDefaults.Height, actualSettings.Height);
    }

    [Fact]
    public void LoadFromJson_ValidJson_ShouldLoadPropertiesSuccessfully()
    {
        // Arrange
        // Write a perfectly valid JSON configuration matching the structure
        string validJsonContent = "{\n  \"Width\": 150,\n  \"Height\": 60,\n  \"Density\": 0.45,\n  \"FpsRate\": 25\n}";
        File.WriteAllText(_tempFilePath, validJsonContent);

        // Act
        GameSettings actualSettings = GameSettings.LoadFromJson(_tempFilePath);

        // Assert
        Assert.NotNull(actualSettings);
        Assert.Equal(150, actualSettings.Width);
        Assert.Equal(60, actualSettings.Height);
        Assert.Equal(0.45, actualSettings.Density);
        Assert.Equal(25, actualSettings.FpsRate);
    }
}