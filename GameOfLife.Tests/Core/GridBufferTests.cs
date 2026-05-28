using GameOfLife.Core;

namespace GameOfLife.Tests.Core;

public class GridBufferTests
{
    [Fact]
    public void Constructor_ValidDimensions_ShouldInitializeCorrectly()
    {
        // Arrange
        int width = 10;
        int height = 5;
        bool toroidal = true;

        // Act
        var buffer = new GridBuffer(width, height, toroidal);

        // Assert
        Assert.Equal(width, buffer.Width);
        Assert.Equal(height, buffer.Height);
        Assert.Equal(toroidal, buffer.Toroidal);
        Assert.Equal(width * height, buffer.Cells.Length);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    [InlineData(-5, 5)]
    public void Constructor_InvalidDimensions_ShouldThrowArgumentOutOfRangeException(int width, int height)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new GridBuffer(width, height, toroidal: false));
    }

    [Fact]
    public void Indexer_SetAndGet_ShouldAccessCorrect1DArrayElement()
    {
        // Arrange
        var buffer = new GridBuffer(width: 4, height: 3, toroidal: false);

        // Act
        // Coordinates (x=2, y=1) maps to index: 1 * 4 + 2 = 6
        buffer[2, 1] = true;

        // Assert
        Assert.True(buffer[2, 1], "The indexer getter failed to retrieve the value that was just set.");
        Assert.True(buffer.Cells[6], "The 1D array layout mapping formula (y * Width + x) is incorrect.");
    }
}