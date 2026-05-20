namespace GameOfLife.Interfaces;

internal interface ICellularRule
{
    string RuleName { get; }

    bool CalculateNextState(bool currentState, int livingNeighbors);
}
