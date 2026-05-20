using System.Collections.Concurrent;

namespace GameOfLife;

internal class SimulationEngine
{
    private GridBuffer _currentGrid;
    private GridBuffer _nextGrid;
    public int Width { get; }
    public int Height { get; }

    public SimulationEngine(int width, int height)
    {
        Width = width;
        Height = height;
        _currentGrid = new GridBuffer(width, height, toroidal: true);
        _nextGrid = new GridBuffer(width, height, toroidal: true);
    }

    public void UpdatePattern()
    {
        // Create a partitioner to divide the rows of the grid into chunks for optimized parallel processing
        var rowPartitioner = Partitioner.Create(0, Height);

        Parallel.ForEach(rowPartitioner, range =>
        {
            for (int y = range.Item1; y < range.Item2; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int liveNeighbors = _currentGrid.CountLivingNeighbours(x, y);
                    if (_currentGrid[x,y]) // TODO: replace with external rule
                    {
                        _nextGrid[x,y] = (liveNeighbors == 2 || liveNeighbors == 3);
                    }
                    else
                    {
                        _nextGrid[x,y] = (liveNeighbors == 3);
                    }
                }
            }
        });

        (_nextGrid, _currentGrid) = (_currentGrid, _nextGrid);  // pointer swap to avoid copying arrays
    }
}
