// Provide a grid of booleans to store the living status of cells
// and a method to get the number of living neighbours of a cell.

namespace GameOfLife;

internal class GridBuffer
{
    public int Width { get; }
    public int Height { get; }
    public bool Torodial { get; }
    private readonly bool[] _cells;  // true = alive, false = dead

    public GridBuffer(int width, int height, bool torodial)
    {
        Width = width;
        Height = height;
        Torodial = torodial;
        _cells = new bool[Width * Height];
    }

    public bool this[int x, int y]  // indexer to access the grid conveniently by x and y
    {
        get => _cells[y * Width + x];
        set => _cells[y * Width + x] = value;
    }

    public int CountLivingNeighbours(int x, int y)
    {
        return Torodial 
            ? CountLivingNeighboursTorodial(x, y) 
            : CountLivingNeighboursBordered(x, y);
    }

    private int CountLivingNeighboursTorodial(int x, int y)
    {
        int count = 0;
        for (int deltaY = -1; deltaY <= 1; deltaY++)
        {
            int checkY = (y + deltaY + Height) % Height;
            for (int deltaX = -1; deltaX <= 1; deltaX++)
            {
                if (deltaX == 0 && deltaY == 0) continue;  // skip the center cell
                int checkX = (x + deltaX + Width) % Width;
                if (this[checkX, checkY]) count++;
            }
        }
        return count;
    }
    private int CountLivingNeighboursBordered(int x, int y)
    {
        int count = 0;
        int startX = x <= 0 ? 0 : x - 1;
        int stopX = x >= Width - 1 ? Width - 1 : x + 1;
        int startY = y <= 0 ? 0 : y - 1;
        int stopY = y >= Height - 1 ? Height - 1 : y + 1;
        for (int checkY = startY; checkY <= stopY; checkY++)
        {
            for (int checkX = startX; checkX <= stopX; checkX++)
            {
                if (checkX == x && checkY == y) continue;  // skip the center cell
                if (this[checkX, checkY]) count++;
            }
        }
        return count;
    }
}