using GameOfLife.CellurlarRules;
using GameOfLife.Interfaces;
using GameOfLife.Neighbourhoods;

namespace GameOfLife;

internal class SimulationEngine
{
    private GridBuffer _currentGrid;
    private GridBuffer _nextGrid;

    private readonly Action _updateMethod;  // cache the method to avoid virtual calls on every update

    public SimulationEngine(GameSettings settings)
    {
        _currentGrid = new GridBuffer(settings.Width, settings.Height, settings.Toroidal);
        _nextGrid = new GridBuffer(settings.Width, settings.Height, settings.Toroidal);
        _updateMethod = ResolveUpdateMethod(settings.RuleType, settings.NeighbourType);
    }

    public void UpdatePattern()
    {
        _updateMethod();
    }

    public void CopySnapshot(bool[] targetCells, int viewWidth, int viewHeight)
    {
        bool[] currentCells = _currentGrid.Cells;
        int gridWidth = _currentGrid.Width;  // assure a constant value (for JIT optimization) and avoid closures in the loop
        int limitX = Math.Min(viewWidth, _currentGrid.Width);
        int limitY = Math.Min(viewHeight, _currentGrid.Height);

        Parallel.For(0, limitY, y =>
        {
            int sourceOffset = y * gridWidth;
            int targetOffset = y * viewWidth;
            Array.Copy(currentCells, sourceOffset, targetCells, targetOffset, limitX);
        });
    }

    private Action ResolveUpdateMethod(CellularRuleType ruleType, NeighbourhoodType neighbourType) =>
    (ruleType, neighbourType) switch
    {
        (CellularRuleType.Conway, NeighbourhoodType.Moore) =>
            UpdatePatternGeneric<ConwayRule, MooreNeighbourhood>,
        (CellularRuleType.Conway, NeighbourhoodType.VonNeumann) =>
            UpdatePatternGeneric<ConwayRule, VonNeumannNeighbourhood>,
        (CellularRuleType.HighLife, NeighbourhoodType.Moore) =>
            UpdatePatternGeneric<HighLifeRule, MooreNeighbourhood>,
        (CellularRuleType.HighLife, NeighbourhoodType.VonNeumann) =>
            UpdatePatternGeneric<HighLifeRule, VonNeumannNeighbourhood>,
        _ => throw new ArgumentException("Invalid combination of rule and neighbourhood.")
    };

    private void UpdatePatternGeneric<TRule, TStrategy>()
        // because of the struct constraints, the JIT compiler can inline the method and optimize it heavily
        where TRule : struct, ICellularRule
        where TStrategy : struct, INeighbourhoodStrategy
    {
        // create an instance without heap allocation, since it's a struct
        TRule rule = default;
        TStrategy strategy = default;

        // 1. Class fields and properties are stored on the heap where local variables are stored on the (fast) stack. 
        // 2. With immutable local variables, loops can be unrolled by the JIT compiler.
        // 3. Using a local int instead of a complex object which needs to be put into a "closure" can improve performance.
        int width = _currentGrid.Width;
        int height = _currentGrid.Height;
        GridBuffer currentGrid = _currentGrid;  // we need the whole grid to count neighbours
        bool[] currentCells = currentGrid.Cells;
        bool[] nextCells = _nextGrid.Cells;  // we only need a reference to the array to store the next state

        // since the grid is stored in a linear array, one row per thread is contiguous data in memory
        Parallel.For(0, height, y =>
        {
            int rowOffset = y * width;
            for (int x = 0; x < width; x++)
            {
                int gridIndex = rowOffset + x;
                int liveNeighbours = strategy.CountNeighbours(currentGrid, x, y);
                nextCells[gridIndex] = rule.CalculateNextState(currentCells[gridIndex], liveNeighbours);
            }
        });
        (_nextGrid, _currentGrid) = (_currentGrid, _nextGrid);  // pointer swap to avoid copying arrays
    }
}
