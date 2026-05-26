// Count all 4 living orthogonal neighbours of a cell, excluding diagonals. (Von Neumann Strategy)

using GameOfLife.Core;
using GameOfLife.Interfaces;

namespace GameOfLife.Neighbourhoods;

internal struct VonNeumannNeighbourhood : INeighbourhoodStrategy
{
    public static int MaxNeighbours => 4;  // needed for statistics display

    public readonly int CountNeighbours(GridBuffer grid, int x, int y)
    {
        return grid.Toroidal  // Will be optimized by the JIT compiler to a constant branch
            ? CountNeighboursToroidal(grid, x, y)
            : CountNeighboursBordered(grid, x, y);
    }

    private static int CountNeighboursToroidal(GridBuffer grid, int x, int y)
    {
        int width = grid.Width;
        int height = grid.Height;
        ReadOnlySpan<bool> cells = grid.Cells;

        // Modulo is slow, we will use conditional logic to wrap around the edges
        int leftX = (x == 0) ? width - 1 : x - 1;
        int rightX = (x == width - 1) ? 0 : x + 1;
        int upY = (y == 0) ? height - 1 : y - 1;
        int downY = (y == height - 1) ? 0 : y + 1;

        // Calculate the row offsets in the 1D array
        int rowUp = upY * width;
        int rowCurrent = y * width;
        int rowDown = downY * width;

        int count = 0;

        // 4 orthogonal checks (branchless conversion to numbers)
        count += cells[rowUp + x] ? 1 : 0;
        count += cells[rowCurrent + leftX] ? 1 : 0;
        count += cells[rowCurrent + rightX] ? 1 : 0;
        count += cells[rowDown + x] ? 1 : 0;

        return count;
    }

    private static int CountNeighboursBordered(GridBuffer grid, int x, int y)
    {
        int width = grid.Width;
        ReadOnlySpan<bool> cells = grid.Cells;

        // pre-check if we are at the borders (branchless/CMOV optimized)
        bool hasUp = y > 0;
        bool hasDown = y < grid.Height - 1;
        bool hasLeft = x > 0;
        bool hasRight = x < width - 1;

        int rowCurrent = y * width;
        int count = 0;

        count += (hasUp && cells[rowCurrent - width + x]) ? 1 : 0;  
        count += (hasLeft && cells[rowCurrent + (x - 1)]) ? 1 : 0;  
        count += (hasRight && cells[rowCurrent + (x + 1)]) ? 1 : 0; 
        count += (hasDown && cells[rowCurrent + width + x]) ? 1 : 0;

        return count;
    }
}
