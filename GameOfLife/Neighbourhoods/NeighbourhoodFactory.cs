namespace GameOfLife.Neighbourhoods;

internal class NeighbourhoodFactory
{
    public static int GetMaxNeighbours(NeighbourhoodType type)
    {
        return type switch
        {
            NeighbourhoodType.Moore => MooreNeighbourhood.MaxNeighbours,
            NeighbourhoodType.VonNeumann => VonNeumannNeighbourhood.MaxNeighbours,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown type: {type}")
        };
    }
}
