// This class implements the rules of Conway's Game of Life using bit manipulation for efficient state calculation.
// It's (probably) the fastest way to determine the next state of a cell because it operates entirely within CPU 
// registers, eliminating branching and memory overhead (arrays, lists, etc.).

using GameOfLife.Interfaces;
using System.Runtime.CompilerServices;

namespace GameOfLife.CellurlarRules;

internal struct ConwayRule : ICellularRule  // struct is used to avoid heap allocation
{
    public readonly string RuleName => "Conway's Game of Life";

    // Bitmasks representing survival and birth rules:
    // 76543210 <- number of living neighbours
    // 00001000 <- a cell is born if it has exactly 3 neighbours (bit 3)
    private const int BornMask = (1 << 3);              
    // 00001100 <- a cell survives if it has 2 or 3 neighbours (bits 2 and 3)
    private const int SurviveMask = (1 << 2) | (1 << 3);

    private const int XorMask = BornMask ^ SurviveMask;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]  // Force JIT to inline this method into the caller
    public readonly bool CalculateNextState(bool currentState, int livingNeighbours)
    {
        // Shift bit 1 left by the neighbor count and perform a bitwise AND against the mask.
        int neighbourBit = 1 << livingNeighbours;

        // Read currentState as byte (0 or -1) directly from the register without any branching (within 1 CPU cycle)
        int stateSign = -Unsafe.As<bool, byte>(ref currentState);
        int mask = BornMask ^ (stateSign & XorMask);
        return (neighbourBit & mask) != 0;
    }
}
