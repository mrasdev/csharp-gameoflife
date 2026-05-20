using GameOfLife.Interfaces;

namespace GameOfLife.Neighbourhoods;

internal enum NeighbourhoodType
{
    Moore,
    VonNeumann
}

internal static class NeighbourhoodFactory
{
    public static INeighbourhoodStrategy Create(NeighbourhoodType type)
    {
        return type switch
        {
            NeighbourhoodType.Moore => default(MooreNeighbourhood),
            NeighbourhoodType.VonNeumann => default(VonNeumannNeighbourhood),
            _ => throw new ArgumentException($"Unknown neighbourhood type: {type}")
        };
    }
}
