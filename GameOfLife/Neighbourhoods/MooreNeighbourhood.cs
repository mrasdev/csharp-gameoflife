// Count all 8 living neighbours of a cell, including diagonals. (Moore Strategy)

using GameOfLife.Interfaces;

namespace GameOfLife.Neighbourhoods;

internal struct MooreNeighbourhood : INeighbourhoodStrategy
{
    public int CountNeighbours(GridBuffer grid,int x, int y)
    {
        return grid.Toroidal  // Will be optimized by the JIT compiler to a constant branch
            ? CountNeighboursToroidal(grid, x, y)
            : CountNeighboursBordered(grid, x, y);
    }

    private int CountNeighboursToroidal(GridBuffer grid, int x, int y)
    {
        // Modulo is slow, we will use conditional logic to wrap around the edges
        int leftX = (x == 0) ? grid.Width - 1 : x - 1;
        int rightX = (x == grid.Width - 1) ? 0 : x + 1;
        int upY = (y == 0) ? grid.Height - 1 : y - 1;
        int downY = (y == grid.Height - 1) ? 0 : y + 1;

        // Calculate the row offsets in the 1D array
        int rowUp = upY * grid.Width;
        int rowCurrent = y * grid.Width;
        int rowDown = downY * grid.Width;

        int count = 0;

        // 8 if statements are faster than loops for a fixed number of iterations
        if (grid.Cells[rowUp + leftX]) count++;
        if (grid.Cells[rowUp + x]) count++;
        if (grid.Cells[rowUp + rightX]) count++;
        if (grid.Cells[rowCurrent + leftX]) count++;
        if (grid.Cells[rowCurrent + rightX]) count++;
        if (grid.Cells[rowDown + leftX]) count++;
        if (grid.Cells[rowDown + x]) count++;
        if (grid.Cells[rowDown + rightX]) count++;

        return count;
    }
    private int CountNeighboursBordered(GridBuffer grid, int x, int y)
    {
        // Limit the neighbour coordinates to the grid boundaries to avoid out-of-bounds access
        int leftX = x <= 0 ? x : x - 1;
        int rightX = x >= grid.Width - 1 ? x : x + 1;
        int upY = y <= 0 ? y : y - 1;
        int downY = y >= grid.Height - 1 ? y : y + 1;

        int rowUp = upY * grid.Width;
        int rowCurrent = y * grid.Width;
        int rowDown = downY * grid.Width;

        int count = 0;

        // Skip counting the current cell and the duplicates at the bounds
        if (upY != y)
        {
            if (leftX != x) if (grid.Cells[rowUp + leftX]) count++;
            if (grid.Cells[rowUp + x]) count++;
            if (rightX != x) if (grid.Cells[rowUp + rightX]) count++;
        }

        if (leftX != x) if (grid.Cells[rowCurrent + leftX]) count++;
        if (rightX != x) if (grid.Cells[rowCurrent + rightX]) count++;

        if (downY != y)
        {
            if (leftX != x) if (grid.Cells[rowDown + leftX]) count++;
            if (grid.Cells[rowDown + x]) count++;
            if (rightX != x) if (grid.Cells[rowDown + rightX]) count++;
        }

        return count;
    }
}
