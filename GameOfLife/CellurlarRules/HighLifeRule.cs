// This class implements the HighLife rules using bit manipulation for efficient state calculation.
// It's the fastest way to determine the next state of a cell because it's using CPU registers only, without
// any branching or memory access (arrays, lists, etc.).

using GameOfLife.Interfaces;

namespace GameOfLife.CellurlarRules;

internal class HighLifeRule : ICellularRule
{
    public string RuleName => "HighLife";

    // Using bit masks to represent the rules for survival and birth:
    // 76543210 <- number of living neighbours
    // 01001000 <- a cell is born if it has exactly 3 or 6 neighbours (bits 3 and 6)
    private const int BornMask = (1 << 3) | (1 << 6);
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
