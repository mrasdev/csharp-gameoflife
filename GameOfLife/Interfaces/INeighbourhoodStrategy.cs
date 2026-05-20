namespace GameOfLife.Interfaces;

internal interface INeighbourhoodStrategy
{
    int CountNeighbours(GridBuffer grid, int x, int y);
}
