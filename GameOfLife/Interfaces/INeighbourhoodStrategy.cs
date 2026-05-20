namespace GameOfLife.Interfaces;

internal interface INeighbourhoodStrategy
{
    int CountNeighbours(int x, int y);
}
