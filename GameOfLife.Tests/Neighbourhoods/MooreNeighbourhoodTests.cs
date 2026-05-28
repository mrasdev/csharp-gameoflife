using GameOfLife.Core;
using GameOfLife.Neighbourhoods;

namespace GameOfLife.Tests.Neighbourhoods;

public class MooreNeighbourhoodTests
{
    [Fact]
    public void CountNeighbours_ToroidalWrapping_ShouldWrapAroundEdgesCorrectly()
    {
        // Arrange
        // Create a 3x3 toroidal grid
        var buffer = new GridBuffer(width: 3, height: 3, toroidal: true);
        var strategy = new MooreNeighbourhood();

        // Place a living cell at the top-left corner (0,0)
        buffer[0, 0] = true;

        // Act
        // Check neighbors of the bottom-right corner (2,2)
        // In a toroidal grid, (0,0) is a direct neighbor of (2,2)
        int count = strategy.CountNeighbours(buffer, 2, 2);

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public void CountNeighbours_BorderedGrid_ShouldNotWrapAroundEdges()
    {
        // Arrange
        // Create a 3x3 standard bounded grid (non-toroidal)
        var buffer = new GridBuffer(width: 3, height: 3, toroidal: false);
        var strategy = new MooreNeighbourhood();

        // Place a living cell at the top-left corner (0,0)
        buffer[0, 0] = true;

        // Act
        // Check neighbors of the bottom-right corner (2,2)
        int count = strategy.CountNeighbours(buffer, 2, 2);

        // Assert
        Assert.Equal(0, count);
    }
}