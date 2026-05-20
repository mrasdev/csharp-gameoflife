using GameOfLife.Neighbourhoods;

namespace GameOfLife;

internal class GameSettings
{
    public int Width { get; set; } = 100;        // Standardwerte, falls im JSON was fehlt
    public int Height { get; set; } = 40;
    public bool Toroidal { get; set; } = true;
    public CellularRuleType RuleType { get; set; } = CellularRuleType.Conway;
    public NeighbourhoodType NeighbourType { get; set; } = NeighbourhoodType.Moore;
}