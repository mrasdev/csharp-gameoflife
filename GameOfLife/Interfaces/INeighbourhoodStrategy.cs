namespace GameOfLife.Interfaces;

internal interface INeighbourhoodStrategy
{
    static abstract int MaxNeighbours { get; }
    int CountNeighbours(GridBuffer grid, int x, int y);
}
