// Provide a grid of booleans to store the living status of cells and a method to get the number
// of living neighbours of a cell.
// By default, Moore neighbourhood is used, but the strategy can be changed during runtime.

using GameOfLife.Interfaces;
using GameOfLife.Neighbourhoods;

namespace GameOfLife;

internal class GridBuffer
{
    // These properties must be readonly, as they will be accessed frequently during neighbour counting (JIT optimization)
    public int Width { get; }
    public int Height { get; }
    public bool Toroidal { get; }

    private readonly bool[] _cells;  // true = alive, false = dead
    private INeighbourhoodStrategy _strategy;

    public GridBuffer(int width, int height, bool toroidal)
    {
        Width = width;
        Height = height;
        Toroidal = toroidal;
        _cells = new bool[Width * Height];
        _strategy = new MooreNeighbourhood(this);  // default to Moore neighbourhood, can be changed later
    }

    public bool this[int x, int y]  // indexer to access the grid conveniently by x and y (but slower)
    {
        get => _cells[y * Width + x];
        set => _cells[y * Width + x] = value;
    }

    public ReadOnlySpan<bool> Cells => _cells;  // same performance as exposing the array directly, but safer

    public void SetNeighbourhoodStrategy(INeighbourhoodStrategy strategy)
    {
        _strategy = strategy;
    }

    public int CountLivingNeighbours(int x, int y)
    {
        return _strategy.CountNeighbours(x, y);
    }
}