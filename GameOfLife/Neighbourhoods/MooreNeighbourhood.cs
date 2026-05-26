// Count all 8 living neighbours of a cell, including diagonals. (Moore Strategy)

using GameOfLife.Core;
using GameOfLife.Interfaces;

namespace GameOfLife.Neighbourhoods;

internal struct MooreNeighbourhood : INeighbourhoodStrategy
{
    public static int MaxNeighbours => 8;  // needed for statistics display

    public readonly int CountNeighbours(GridBuffer grid, int x, int y)
    {
        return grid.Toroidal  // will be optimized by the JIT compiler to a constant branch
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

        // conversion to numbers is fast (branchless)
        count += cells[rowUp + leftX] ? 1 : 0;
        count += cells[rowUp + x] ? 1 : 0;
        count += cells[rowUp + rightX] ? 1 : 0;
        count += cells[rowCurrent + leftX] ? 1 : 0;
        count += cells[rowCurrent + rightX] ? 1 : 0;
        count += cells[rowDown + leftX] ? 1 : 0;
        count += cells[rowDown + x] ? 1 : 0;
        count += cells[rowDown + rightX] ? 1 : 0;

        return count;
    }

    private static int CountNeighboursBordered(GridBuffer grid, int x, int y)
    {
        int width = grid.Width;
        ReadOnlySpan<bool> cells = grid.Cells;

        // reduce the 3x3 window at the borders
        int startX = Math.Max(0, x - 1);
        int endX = Math.Min(width - 1, x + 1);
        int startY = Math.Max(0, y - 1);
        int endY = Math.Min(grid.Height - 1, y + 1);
        int count = 0;

        for (int rowY = startY; rowY <= endY; rowY++)  // loops will be unrolled by JIT compiler
        {
            int rowOffset = rowY * width;
            for (int colX = startX; colX <= endX; colX++)
            {
                if (colX == x && rowY == y) continue;  // skip center cell
                if (cells[rowOffset + colX]) count++;
            }
        }

        return count;
    }
}
