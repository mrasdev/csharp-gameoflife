using GameOfLife.Core;

namespace GameOfLife.Tests.Core;

public class GridBufferTests
{
    // ==========================================
    // 1. CONSTRUCTOR TESTS
    // ==========================================

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
    public void Constructor_NewBuffer_ShouldBeCompletelyDead()
    {
        // Arrange & Act
        var buffer = new GridBuffer(width: 5, height: 5, toroidal: false);

        // Assert
        // Verify all cells are initialized to false (dead)
        Assert.All(buffer.Cells, Assert.False);
    }

    // ==========================================
    // 2. INDEXER TESTS (MAPPING & BOUNDARIES)
    // ==========================================

    [Fact]
    public void Indexer_SetAndGet_ShouldAccessCorrect1DArrayElement()
    {
        // Arrange
        var buffer = new GridBuffer(width: 4, height: 3, toroidal: false);

        // Act
        // Coordinates (x=2, y=1) maps to index: 1 * 4 + 2 = 6
        buffer[2, 1] = true;

        // Assert
        Assert.True(buffer[2, 1]);
        Assert.True(buffer.Cells[6]);
    }

    [Fact]
    public void Indexer_CornerCoordinates_ShouldMapToFirstAndLastArrayElements()
    {
        // Arrange
        int width = 4;
        int height = 3;
        var buffer = new GridBuffer(width, height, toroidal: false);

        // Act & Assert for Top-Left (0,0) -> Index 0
        buffer[0, 0] = true;
        Assert.True(buffer.Cells[0]);

        // Act & Assert for Bottom-Right (Width-1, Height-1) -> Index (Width * Height - 1)
        buffer[width - 1, height - 1] = true;
        int lastIndex = (width * height) - 1;
        Assert.True(buffer.Cells[lastIndex]);
    }

    // ==========================================
    // 3. SETCELLS METHOD TESTS
    // ==========================================

    [Fact]
    public void SetCells_ValidSpanLength_ShouldCopyDataCorrectly()
    {
        // Arrange
        var buffer = new GridBuffer(width: 3, height: 2, toroidal: false); // Total 6 cells
        bool[] samplePattern = [true, false, true, false, true, false];

        // Act
        buffer.SetCells(samplePattern.AsSpan());

        // Assert
        Assert.Equal(samplePattern, buffer.Cells);
        // Verify indexer works with the new data
        Assert.True(buffer[0, 0]); // Index 0
        Assert.False(buffer[1, 0]); // Index 1
        Assert.False(buffer[0, 1]); // Index 3 (1 * 3 + 0)
        Assert.True(buffer[1, 1]); // Index 4 (1 * 3 + 1)
    }

    [Theory]
    [InlineData(5)] // Too short (Grid requires 6)
    [InlineData(7)] // Too long (Grid requires 6)
    public void SetCells_InvalidSpanLength_ShouldThrowArgumentOutOfRangeException(int invalidLength)
    {
        // Arrange
        var buffer = new GridBuffer(width: 3, height: 2, toroidal: false); // Total 6 cells
        bool[] invalidPattern = new bool[invalidLength];

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.SetCells(invalidPattern.AsSpan()));
    }
}