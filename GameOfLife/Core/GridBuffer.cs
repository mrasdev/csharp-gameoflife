// Provide a grid of booleans to store the living status of cells.

namespace GameOfLife.Core;

internal class GridBuffer
{
    // These properties must be readonly, as they will be accessed frequently during neighbour counting (JIT optimization)
    public int Width { get; }
    public int Height { get; }
    public bool Toroidal { get; }

    public readonly bool[] Cells;  // true = alive, false = dead

    public GridBuffer(int width, int height, bool toroidal)
    {
        Width = width;
        Height = height;
        Toroidal = toroidal;
        Cells = new bool[Width * Height];
    }

    public bool this[int x, int y]  // indexer to access the grid conveniently by x and y (but slower)
    {
        get => Cells[y * Width + x];
        set => Cells[y * Width + x] = value;
    }

    public void SetCells(bool[] cells)
    {
        if (cells.Length != Cells.Length)
            throw new ArgumentException("Cells size is different to grid size!");
        Array.Copy(cells, Cells, cells.Length);
    }
}