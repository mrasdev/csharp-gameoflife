using GameOfLife.CellurlarRules;
using GameOfLife.Interfaces;
using GameOfLife.Neighbourhoods;

namespace GameOfLife;

internal class SimulationEngine
{
    public int Width => _currentGrid.Width;
    public int Height => _currentGrid.Height;
    public int MaxNeighbours { get; }
    public long GenerationCount => Volatile.Read(ref _generationCount);
    public long UpdatesPerSecond => Volatile.Read(ref _updatesPerSecond);
    public long ThreadsPerSecond => Volatile.Read(ref _threadsPerSecond);
    public long LivingCellsCount => Volatile.Read(ref _livingCellsCount);

    private GridBuffer _currentGrid;
    private GridBuffer _nextGrid;
    private bool[]? _initalCells;

    private readonly Action _updateMethod;  // cache the method to avoid virtual calls on every update
    private readonly object _locker = new();

    // for statistics display
    private long _generationCount;
    private long _updatesThisSecond;
    private long _updatesPerSecond;
    private long _threadsThisSecond;
    private long _threadsPerSecond;
    private long _livingCellsCount;
    private DateTime _lastRateUpdate = DateTime.Now;

    public SimulationEngine(GameSettings settings)
    {
        _currentGrid = new GridBuffer(settings.Width, settings.Height, settings.Toroidal);
        _nextGrid = new GridBuffer(settings.Width, settings.Height, settings.Toroidal);
        _updateMethod = ResolveUpdateMethod(settings.RuleType, settings.NeighbourType);
        MaxNeighbours = NeighbourhoodFactory.GetMaxNeighbours(settings.NeighbourType);
    }

    public void UpdatePattern()
    {
        lock (_locker)
        {
            _updateMethod();
        }
    }

    public void CopySnapshot(bool[] targetCells, int viewWidth, int viewHeight)
    {
        lock (_locker)
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
    }

    public void SetCells(bool[] cells)
    {
        lock (_locker)
        {
            _initalCells = cells.ToArray();  // creates a clone
            _currentGrid.SetCells(cells);
        }
    }

    public void Restart()
    {
        if (_initalCells == null) return;

        lock (_locker)
        {
            _currentGrid.SetCells(_initalCells.ToArray());
            Array.Clear(_nextGrid.Cells, 0, _nextGrid.Cells.Length);
            Interlocked.Exchange(ref _generationCount, 0);
            Interlocked.Exchange(ref _updatesThisSecond, 0);
            Interlocked.Exchange(ref _updatesPerSecond, 0);
            Interlocked.Exchange(ref _threadsThisSecond, 0);
            Interlocked.Exchange(ref _threadsPerSecond, 0);
            long initialLivingCount = 0;
            for (int i = 0; i < _initalCells.Length; i++)
            {
                if (_initalCells[i]) initialLivingCount++;
            }
            _livingCellsCount = initialLivingCount;
            _lastRateUpdate = DateTime.Now;
        }
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
        long localLivingCells = 0;

        // since the grid is stored in a linear array, one row per thread is contiguous data in memory
        Parallel.For(0, height, y =>
        {
            int rowOffset = y * width;
            long threadLivingCells = 0;  // counter per thread to avoid interlocked overhead
            for (int x = 0; x < width; x++)
            {
                int gridIndex = rowOffset + x;
                int liveNeighbours = strategy.CountNeighbours(currentGrid, x, y);
                bool nextState = rule.CalculateNextState(currentCells[gridIndex], liveNeighbours);
                nextCells[gridIndex] = nextState;
                if (nextState) threadLivingCells++;
            }
            Interlocked.Add(ref localLivingCells, threadLivingCells);
            Interlocked.Increment(ref _threadsThisSecond);
        });
        (_nextGrid, _currentGrid) = (_currentGrid, _nextGrid);  // pointer swap to avoid copying arrays
        _livingCellsCount = localLivingCells;
        Interlocked.Increment(ref _generationCount);
        Interlocked.Increment(ref _updatesThisSecond);
        TrackRates();
    }
    private void TrackRates()
    {
        DateTime now = DateTime.Now;
        if ((now - _lastRateUpdate).TotalSeconds >= 1.0)
        {
            _updatesPerSecond = _updatesThisSecond;
            _updatesThisSecond = 0;
            _threadsPerSecond = _threadsThisSecond;
            _threadsThisSecond = 0;
            _lastRateUpdate = now;
        }
    }
}
