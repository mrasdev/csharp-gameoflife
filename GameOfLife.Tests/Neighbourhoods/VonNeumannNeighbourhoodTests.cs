using GameOfLife.Core;
using GameOfLife.Neighbourhoods;

namespace GameOfLife.Tests.Neighbourhoods;

public class VonNeumannNeighbourhoodTests
{
    private readonly VonNeumannNeighbourhood _strategy = new();

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
    public void CountNeighbours_MiddleCellFullySurrounded_ShouldReturnFour()
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
        // CRITICAL: Von Neumann only checks 4 orthogonal directions (N, S, E, W), completely ignoring diagonals!
        Assert.Equal(4, actualCount);
    }

    [Fact]
    public void CountNeighbours_MiddleCellWithOnlyDiagonalNeighbours_ShouldReturnZero()
    {
        // Arrange
        var buffer = new GridBuffer(width: 3, height: 3, toroidal: false);

        // Place living cells ONLY on the 4 diagonals
        buffer[0, 0] = true; // Top-Left
        buffer[2, 0] = true; // Top-Right
        buffer[0, 2] = true; // Bottom-Left
        buffer[2, 2] = true; // Bottom-Right

        // Act
        int count = _strategy.CountNeighbours(buffer, 1, 1);

        // Assert
        Assert.Equal(0, count);
    }

    // ==========================================
    // 2. BOUNDED / BORDERED GRID TESTS
    // ==========================================

    [Fact]
    public void CountNeighbours_BorderedGridCorner_ShouldOnlyCountValidInternalCells()
    {
        // Arrange
        var buffer = new GridBuffer(width: 3, height: 3, toroidal: false);

        buffer[1, 0] = true; // Right neighbor of (0,0) -> Orthogonal and valid
        buffer[1, 1] = true; // Diagonal neighbor of (0,0) -> Should be ignored anyway

        // Act
        int count = _strategy.CountNeighbours(buffer, 0, 0);

        // Assert
        Assert.Equal(1, count);
    }

    // ==========================================
    // 3. TOROIDAL WRAPPING TESTS
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
    public void CountNeighbours_ToroidalDiagonalCornerWrapping_ShouldReturnZero()
    {
        // Arrange
        var buffer = new GridBuffer(width: 3, height: 3, toroidal: true);
        buffer[0, 0] = true; // Top-Left corner

        // Act
        // Check neighbors of the Bottom-Right corner (2,2)
        int count = _strategy.CountNeighbours(buffer, 2, 2);

        // Assert
        // In a Von Neumann neighborhood, even with wrapping, (0,0) and (2,2) are strictly diagonal
        // to each other, meaning they share no orthogonal edge. The count must be 0!
        Assert.Equal(0, count);
    }
}