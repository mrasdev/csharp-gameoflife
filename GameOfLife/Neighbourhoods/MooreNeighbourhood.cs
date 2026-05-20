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
        return _grid.Torodial
            ? CountNeighboursTorodial(x, y)
            : CountNeighboursBordered(x, y);
    }

    private int CountNeighboursTorodial(int x, int y)
    {
        int count = 0;
        for (int deltaY = -1; deltaY <= 1; deltaY++)
        {
            int checkY = (y + deltaY + _grid.Height) % _grid.Height;
            for (int deltaX = -1; deltaX <= 1; deltaX++)
            {
                if (deltaX == 0 && deltaY == 0) continue;  // skip the center cell
                int checkX = (x + deltaX + _grid.Width) % _grid.Width;
                if (_grid[checkX, checkY]) count++;
            }
        }
        return count;
    }
    private int CountNeighboursBordered(int x, int y)
    {
        int count = 0;
        int startX = x <= 0 ? 0 : x - 1;
        int stopX = x >= _grid.Width - 1 ? _grid.Width - 1 : x + 1;
        int startY = y <= 0 ? 0 : y - 1;
        int stopY = y >= _grid.Height - 1 ? _grid.Height - 1 : y + 1;
        for (int checkY = startY; checkY <= stopY; checkY++)
        {
            for (int checkX = startX; checkX <= stopX; checkX++)
            {
                if (checkX == x && checkY == y) continue;  // skip the center cell
                if (_grid[checkX, checkY]) count++;
            }
        }
        return count;
    }
}
