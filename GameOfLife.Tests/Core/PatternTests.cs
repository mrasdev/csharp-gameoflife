using GameOfLife.Core;

namespace GameOfLife.Tests.Core;

public class PatternTests : IDisposable
{
    private readonly string _tempFilePath;

    public PatternTests()
    {
        // Creates a unique temporary file path for each individual test run
        _tempFilePath = Path.GetTempFileName();
    }

    public void Dispose()
    {
        // Cleanup: Ensures the temporary file is deleted after the test finishes
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }

    // ==========================================
    // 1. RANDOM PATTERN TESTS
    // ==========================================

    [Fact]
    public void CreateRandom_ValidDimensions_ShouldReturnCorrectArrayLength()
    {
        // Arrange
        int width = 10;
        int height = 5;
        double density = 0.3;

        // Act
        bool[] cells = Pattern.CreateRandom(width, height, density);

        // Assert
        Assert.Equal(width * height, cells.Length);
    }

    // ==========================================
    // 2. RLE PARSING SUCCESS TESTS
    // ==========================================

    [Fact]
    public void LoadFromRleFile_ValidSimpleRle_ShouldParseCorrectDimensionsAndCells()
    {
        // Arrange
        // Represents a 3x3 grid:
        // Row 0: o, b, b -> (ob$)
        // Row 1: o, o, b -> (2o$)
        // Row 2: b, b, b -> (3b!)
        string[] rleContent = [
            "x = 3, y = 3, rule = B3/S23",
            "ob$",
            "2o$",
            "3b!"
        ];
        File.WriteAllLines(_tempFilePath, rleContent);

        // Act
        Grid grid = Pattern.LoadFromRleFile(_tempFilePath);

        // Assert
        Assert.Equal(3, grid.Width);
        Assert.Equal(3, grid.Height);
        Assert.Equal(9, grid.Cells.Length);

        // Verify specific cell mapping (y * Width + x)
        Assert.True(grid.Cells[0 * 3 + 0]);  // Row 0, Col 0: 'o' -> True
        Assert.False(grid.Cells[0 * 3 + 1]); // Row 0, Col 1: 'b' -> False
        Assert.True(grid.Cells[1 * 3 + 0]);  // Row 1, Col 0: '2o' first -> True
        Assert.True(grid.Cells[1 * 3 + 1]);  // Row 1, Col 1: '2o' second -> True
        Assert.False(grid.Cells[2 * 3 + 0]); // Row 2, Col 0: '3b' -> False
    }

    [Fact]
    public void LoadFromRleFile_WithComments_ShouldIgnoreCommentsAndParseSuccessfully()
    {
        // Arrange
        string[] rleContent = [
            "#N Glider",
            "#O Richard K. Guy",
            "x = 3, y = 3",
            "3o!"
        ];
        File.WriteAllLines(_tempFilePath, rleContent);

        // Act
        Grid grid = Pattern.LoadFromRleFile(_tempFilePath);

        // Assert
        Assert.Equal(3, grid.Width);
        Assert.Equal(3, grid.Height);
        Assert.True(grid.Cells[0]);
        Assert.True(grid.Cells[1]);
        Assert.True(grid.Cells[2]);
    }

    // ==========================================
    // 3. RLE PARSING ERROR / EXCEPTION TESTS
    // ==========================================

    [Fact]
    public void LoadFromRleFile_InvalidSymbol_ShouldThrowFormatException()
    {
        // Arrange
        string[] rleContent = [
            "x = 3, y = 3",
            "2oX!" // 'X' is an invalid character in RLE formatting
        ];
        File.WriteAllLines(_tempFilePath, rleContent);

        // Act & Assert
        Assert.Throws<FormatException>(() => Pattern.LoadFromRleFile(_tempFilePath));
    }

    [Fact]
    public void LoadFromRleFile_MissingExclamationMark_ShouldThrowFormatException()
    {
        // Arrange
        string[] rleContent = [
            "x = 3, y = 3",
            "3o" // Syntactically invalid because the ending '!' is missing
        ];
        File.WriteAllLines(_tempFilePath, rleContent);

        // Act & Assert
        Assert.Throws<FormatException>(() => Pattern.LoadFromRleFile(_tempFilePath));
    }

    [Fact]
    public void LoadFromRleFile_ExceedWidth_ShouldThrowIndexOutOfRangeException()
    {
        // Arrange
        string[] rleContent = [
            "x = 2, y = 2",
            "3o!" // 3 living cells exceeds the header definition of width = 2
        ];
        File.WriteAllLines(_tempFilePath, rleContent);

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => Pattern.LoadFromRleFile(_tempFilePath));
    }
}