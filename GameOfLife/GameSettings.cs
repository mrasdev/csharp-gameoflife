using GameOfLife.CellurlarRules;
using GameOfLife.Neighbourhoods;

namespace GameOfLife;

internal class GameSettings
{
    // set default values for all properties, so that the user doesn't have to specify all of them in the JSON file
    public int Width { get; set; } = 100;
    public int Height { get; set; } = 40;
    public bool Toroidal { get; set; } = true;
    public CellularRuleType RuleType { get; set; } = CellularRuleType.Conway;
    public NeighbourhoodType NeighbourType { get; set; } = NeighbourhoodType.Moore;
}