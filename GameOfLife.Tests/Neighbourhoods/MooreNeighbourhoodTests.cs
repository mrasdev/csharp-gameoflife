using GameOfLife.Core;
using GameOfLife.Neighbourhoods;

namespace GameOfLife.Tests.Neighbourhoods;

public class MooreNeighbourhoodTests
{
    private readonly MooreNeighbourhood _strategy = new();

    // ==========================================
    // 1. STANDARD CORE LOGIC TESTS (No wrapping)
    // ==========================================

    [Fact]
    public void CountNeighbours_MiddleCellWithNoNeighbours_ShouldReturnZero()
    {
        // Arrange
        var buffer = new GridBuffer(width: 3, height: 3, toroidal: false);
        // All cells are dead

        // Act
        int count = _strategy.CountNeighbours(buffer, 1, 1);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void CountNeighbours_LivingCell_ShouldNotCountItselfAsNeighbour()
    {
        // Arrange
        var buffer = new GridBuffer(width: 3, height: 3, toroidal: false);
        buffer[1, 1] = true; // The target cell itself is alive, but has no neighbors

        // Act
        int count = _strategy.CountNeighbours(buffer, 1, 1);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void CountNeighbours_MiddleCellFullySurrounded_ShouldReturnEight()
    {
        // Arrange
        var buffer = new GridBuffer(width: 3, height: 3, toroidal: false);

        // Fill the entire 3x3 grid with living cells
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                buffer[x, y] = true;
            }
        }

        // Act
        int actualCount = _strategy.CountNeighbours(buffer, 1, 1);

        // Assert
        Assert.Equal(8, actualCount);
    }

    // ==========================================
    // 2. BOUNDED / BORDERED GRID TESTS
    // ==========================================

    [Fact]
    public void CountNeighbours_BorderedGridCorner_ShouldOnlyCountValidInternalCells()
    {
        // Arrange
        var buffer = new GridBuffer(width: 3, height: 3, toroidal: false);

        // Set cells that would be neighbors if it wrapped, plus one internal neighbor
        buffer[1, 0] = true; // Internal neighbor of (0,0)
        buffer[2, 0] = true; // Far away (should not be counted)

        // Act
        int count = _strategy.CountNeighbours(buffer, 0, 0);

        // Assert
        Assert.Equal(1, count);
    }

    // ==========================================
    // 3. TOROIDAL WRAPPING TESTS (All Directions)
    // ==========================================

    [Fact]
    public void CountNeighbours_ToroidalHorizontalWrapping_ShouldWrapLeftAndRightEdges()
    {
        // Arrange
        var buffer = new GridBuffer(width: 3, height: 3, toroidal: true);
        buffer[0, 1] = true; // Far left cell in the middle row

        // Act
        // Check neighbors of the far right cell in the same row (2,1)
        int count = _strategy.CountNeighbours(buffer, 2, 1);

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public void CountNeighbours_ToroidalVerticalWrapping_ShouldWrapTopAndBottomEdges()
    {
        // Arrange
        var buffer = new GridBuffer(width: 3, height: 3, toroidal: true);
        buffer[1, 0] = true; // Top cell in the middle column

        // Act
        // Check neighbors of the bottom cell in the same column (1,2)
        int count = _strategy.CountNeighbours(buffer, 1, 2);

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public void CountNeighbours_ToroidalDiagonalCornerWrapping_ShouldWrapAcrossAllEdges()
    {
        // Arrange
        var buffer = new GridBuffer(width: 3, height: 3, toroidal: true);
        buffer[0, 0] = true; // Top-Left corner

        // Act
        // Check neighbors of the Bottom-Right corner (2,2)
        int count = _strategy.CountNeighbours(buffer, 2, 2);

        // Assert
        Assert.Equal(1, count);
    }
}