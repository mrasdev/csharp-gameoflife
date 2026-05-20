// Count all 8 living neighbours of a cell, including diagonals. (Moore Strategy)

using GameOfLife.Interfaces;

namespace GameOfLife.Neighbourhoods;

internal class MooreNeighbourhood : INeighbourhoodStrategy
{
    private readonly GridBuffer _grid;

    public MooreNeighbourhood(GridBuffer grid)
    {
        _grid = grid;
    }

    public int CountNeighbours(int x, int y)
    {
        return _grid.Toroidal  // Will be optimized by the JIT compiler to a constant branch
            ? CountNeighboursToroidal(x, y)
            : CountNeighboursBordered(x, y);
    }

    private int CountNeighboursToroidal(int x, int y)
    {
        // Modulo is slow, we will use conditional logic to wrap around the edges
        int leftX = (x == 0) ? _grid.Width - 1 : x - 1;
        int rightX = (x == _grid.Width - 1) ? 0 : x + 1;
        int upY = (y == 0) ? _grid.Height - 1 : y - 1;
        int downY = (y == _grid.Height - 1) ? 0 : y + 1;

        // Calculate the row offsets in the 1D array
        int rowUp = upY * _grid.Width;
        int rowCurrent = y * _grid.Width;
        int rowDown = downY * _grid.Width;

        int count = 0;

        // 8 if statements are faster than loops for a fixed number of iterations
        if (_grid.Cells[rowUp + leftX]) count++;
        if (_grid.Cells[rowUp + x]) count++;
        if (_grid.Cells[rowUp + rightX]) count++;
        if (_grid.Cells[rowCurrent + leftX]) count++;
        if (_grid.Cells[rowCurrent + rightX]) count++;
        if (_grid.Cells[rowDown + leftX]) count++;
        if (_grid.Cells[rowDown + x]) count++;
        if (_grid.Cells[rowDown + rightX]) count++;

        return count;
    }
    private int CountNeighboursBordered(int x, int y)
    {
        // Limit the neighbour coordinates to the grid boundaries to avoid out-of-bounds access
        int leftX = x <= 0 ? x : x - 1;
        int rightX = x >= _grid.Width - 1 ? x : x + 1;
        int upY = y <= 0 ? y : y - 1;
        int downY = y >= _grid.Height - 1 ? y : y + 1;

        int rowUp = upY * _grid.Width;
        int rowCurrent = y * _grid.Width;
        int rowDown = downY * _grid.Width;

        int count = 0;

        // Skip counting the current cell and the duplicates at the bounds
        if (upY != y)
        {
            if (leftX != x) if (_grid.Cells[rowUp + leftX]) count++;
            if (_grid.Cells[rowUp + x]) count++;
            if (rightX != x) if (_grid.Cells[rowUp + rightX]) count++;
        }

        if (leftX != x) if (_grid.Cells[rowCurrent + leftX]) count++;
        if (rightX != x) if (_grid.Cells[rowCurrent + rightX]) count++;

        if (downY != y)
        {
            if (leftX != x) if (_grid.Cells[rowDown + leftX]) count++;
            if (_grid.Cells[rowDown + x]) count++;
            if (rightX != x) if (_grid.Cells[rowDown + rightX]) count++;
        }

        return count;
    }
}
