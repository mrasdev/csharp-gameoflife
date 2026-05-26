// Provide a grid of booleans to store the living status of cells.

namespace GameOfLife.Core;

internal class GridBuffer
{
    public int Width { get; }
    public int Height { get; }
    public bool Toroidal { get; }

    public readonly bool[] Cells;  // true = alive, false = dead

    public GridBuffer(int width, int height, bool toroidal)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0);
        Width = width;
        Height = height;
        Toroidal = toroidal;
        Cells = new bool[width * height];
    }

    public bool this[int x, int y]  // indexer to access the grid conveniently by x and y (but slower)
    {
        get => Cells[y * Width + x];
        set => Cells[y * Width + x] = value;
    }

    public void SetCells(ReadOnlySpan<bool> newCells)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(newCells.Length, Cells.Length);
        newCells.CopyTo(Cells);
    }
}