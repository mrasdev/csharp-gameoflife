// This class implements the rules of Conway's Game of Life using bit manipulation for efficient state calculation.
// It's the fastest way to determine the next state of a cell because it's using CPU registers only, without
// any branching or memory access (arrays, lists, etc.).

using GameOfLife.Interfaces;

namespace GameOfLife.CellurlarRules;

internal struct ConwayRule : ICellularRule  // struct is used to avoid heap allocation
{
    public string RuleName => "Conway's Game of Life";

    // Using bit masks to represent the rules for survival and birth:
    // 76543210 <- number of living neighbours
    // 00001000 <- a cell is born if it has exactly 3 neighbours (bit 3)
    private const int BornMask = (1 << 3);              
    // 00001100 <- a cell survives if it has 2 or 3 neighbours (bits 2 and 3)
    private const int SurviveMask = (1 << 2) | (1 << 3); 

    public bool CalculateNextState(bool currentState, int livingNeighbours)
    {
        // Move the bit 1 to the left by the number of neighbors and check with a bitwise AND against the mask.
        int neighbourBit = 1 << livingNeighbours;

        return currentState
            ? (neighbourBit & SurviveMask) != 0
            : (neighbourBit & BornMask) != 0;
    }
}
