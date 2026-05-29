namespace GameOfLife.Models;

internal readonly record struct SimulationStats(
    long GenerationCount,
    long UpdatesPerSecond,
    long CellsPerSecond,
    long LivingCells,
    int MaxNeighbours
)
{
    public long NeighbourChecksPerSecond
        => CellsPerSecond * MaxNeighbours;
}