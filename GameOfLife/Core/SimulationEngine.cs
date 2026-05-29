using GameOfLife.CellurlarRules;
using GameOfLife.Enums;
using GameOfLife.Interfaces;
using GameOfLife.Models;
using GameOfLife.Neighbourhoods;

namespace GameOfLife.Core;

internal class SimulationEngine : IDisposable
{
    public int Width => _currentGrid.Width;
    public int Height => _currentGrid.Height;

    private readonly GameSettings _settings;
    private GridBuffer _currentGrid;
    private GridBuffer _nextGrid;
    private readonly bool[] _initialCells;

    private readonly Lock _updateLock = new();  // fast lock for cell updates

    // for statistics display
    private long _generationCount;
    private long _updatesThisSecond;
    private long _updatesPerSecond;
    private long _cellsThisSecond;
    private long _cellsPerSecond;
    private long _livingCellsCount;
    private readonly int _maxNeighbours;
    private long _lastRateTimestamp = Environment.TickCount64;

    private readonly int _threadCount;
    private readonly Thread[] _workers;
    private readonly Barrier _barrier; // synchronize start and end of workers
    private bool _isDisposed;
    private long _nextLivingCells;

    // Delegate for allocation-less execution
    private delegate void StepDelegate(int startY, int endY);
    private StepDelegate? _currentStepMethod;

    public SimulationEngine(GameSettings settings)
    {
        _settings = settings;
        _currentGrid = new GridBuffer(settings.Width, settings.Height, settings.Toroidal);
        _nextGrid = new GridBuffer(settings.Width, settings.Height, settings.Toroidal);
        _initialCells = new bool[settings.Width * settings.Height];
        _maxNeighbours = NeighbourhoodFactory.GetMaxNeighbours(settings.NeighbourType);

        ResolveUpdateMethod(settings.RuleType, settings.NeighbourType);
        _threadCount = Environment.ProcessorCount;
        _workers = new Thread[_threadCount];
        _barrier = new Barrier(_threadCount + 1);  // workers + main thread
        InitWorkerThreads();
    }

    public SimulationStats GetStats()
    {
        return new SimulationStats(
            Volatile.Read(ref _generationCount),
            Volatile.Read(ref _updatesPerSecond),
            Volatile.Read(ref _cellsPerSecond),
            Volatile.Read(ref _livingCellsCount),
            _maxNeighbours
        );
    }

    public void UpdatePattern()
    {
        lock (_updateLock)
        {
            _nextLivingCells = 0;
            _barrier.SignalAndWait();  // wake up the workers
            _barrier.SignalAndWait();  // wait for the workers
            (_nextGrid, _currentGrid) = (_currentGrid, _nextGrid);
            Volatile.Write(ref _livingCellsCount, _nextLivingCells);
            Interlocked.Increment(ref _generationCount);
            Interlocked.Increment(ref _updatesThisSecond);
            TrackRates();
        }
    }

    public void CopySnapshot(bool[] targetCells, int viewWidth, int viewHeight)
    {
        lock (_updateLock)
        {
            bool[] currentCells = _currentGrid.Cells;
            int gridWidth = _currentGrid.Width;
            int limitX = Math.Min(viewWidth, _currentGrid.Width);
            int limitY = Math.Min(viewHeight, _currentGrid.Height);

            for (int y = 0; y < limitY; y++)
            {
                Array.Copy(currentCells, y * gridWidth, targetCells, y * viewWidth, limitX);
            }
        }
    }

    public void SetCells(bool[] cells)
    {
        lock (_updateLock)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(cells.Length, _initialCells.Length);
            Array.Copy(cells, _initialCells, cells.Length);
            _currentGrid.SetCells(cells);

            long initialLivingCount = 0;
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i]) initialLivingCount++;
            }
            Volatile.Write(ref _livingCellsCount, initialLivingCount);
        }
    }

    public void Restart()
    {
        lock (_updateLock)
        {
            if (_settings.UseRandomPattern)
            {
                bool[] newCells = Pattern.GetCells(_settings);
                Array.Copy(newCells, _initialCells, newCells.Length);
                _currentGrid.SetCells(newCells);
            }
            else
            {
                _currentGrid.SetCells(_initialCells);
            }

            Array.Clear(_nextGrid.Cells, 0, _nextGrid.Cells.Length);
            Volatile.Write(ref _generationCount, 0);
            Volatile.Write(ref _updatesThisSecond, 0);
            Volatile.Write(ref _updatesPerSecond, 0);
            Volatile.Write(ref _cellsThisSecond, 0);
            Volatile.Write(ref _cellsPerSecond, 0);

            long initialLivingCount = 0;
            for (int i = 0; i < _initialCells.Length; i++)
            {
                if (_initialCells[i]) initialLivingCount++;
            }
            Volatile.Write(ref _livingCellsCount, initialLivingCount);
            _lastRateTimestamp = Environment.TickCount64;
        }
    }

    public void Dispose()
    {
        lock (_updateLock)  // wait for updatePattern()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _barrier.SignalAndWait();
            try
            {
                _barrier.SignalAndWait();
            }
            catch { /* ignore */ }
            _barrier.Dispose();
        }
    }

    private void InitWorkerThreads()
    {
        int rowsPerThread = _currentGrid.Height / _threadCount;
        for (int i = 0; i < _threadCount; i++)
        {
            int threadIndex = i;
            int startY = threadIndex * rowsPerThread;
            // the last thread takes the rest
            int endY = (threadIndex == _threadCount - 1) ? _currentGrid.Height : startY + rowsPerThread;
            _workers[i] = new Thread(() => WorkerLoop(startY, endY))
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            _workers[i].Start();
        }
    }
    private void WorkerLoop(int startY, int endY)
    {
        try
        {
            while (!_isDisposed)
            {
                _barrier.SignalAndWait();  // wait for main thread
                if (_isDisposed) break;
                _currentStepMethod?.Invoke(startY, endY);
                _barrier.SignalAndWait();  // main thread waits for that
            }
        }
        catch (ObjectDisposedException) { /* ignore */ }
    }

    private void ResolveUpdateMethod(CellularRuleType ruleType, NeighbourhoodType neighbourType)
    {
        _currentStepMethod = (ruleType, neighbourType) switch
        {
            (CellularRuleType.Conway, NeighbourhoodType.Moore) => UpdatePatternGeneric<ConwayRule, MooreNeighbourhood>,
            (CellularRuleType.Conway, NeighbourhoodType.VonNeumann) => UpdatePatternGeneric<ConwayRule, VonNeumannNeighbourhood>,
            (CellularRuleType.HighLife, NeighbourhoodType.Moore) => UpdatePatternGeneric<HighLifeRule, MooreNeighbourhood>,
            (CellularRuleType.HighLife, NeighbourhoodType.VonNeumann) => UpdatePatternGeneric<HighLifeRule, VonNeumannNeighbourhood>,
            _ => throw new ArgumentException("Invalid combination of rule and neighbourhood.")
        };
    }

    private void UpdatePatternGeneric<TRule, TStrategy>(int startY, int endY)
    // because of the struct constraints, the JIT compiler can inline the method and optimize it heavily
    where TRule : struct, ICellularRule
    where TStrategy : struct, INeighbourhoodStrategy
    {
        // create an instance without heap allocation, since it's a struct
        TRule rule = default;
        TStrategy strategy = default;

        // We keep the direct array references locally to make the indexing inside the loop ultra-short
        bool[] currentCells = _currentGrid.Cells;
        bool[] nextCells = _nextGrid.Cells;
        int width = _currentGrid.Width;
        long localLivingCells = 0;

        // since the grid is stored in a linear array, one row per thread is contiguous data in memory
        for (int y = startY; y < endY; y++)
        {
            int rowOffset = y * width;
            for (int x = 0; x < width; x++)
            {
                int gridIndex = rowOffset + x;
                int liveNeighbours = strategy.CountNeighbours(_currentGrid, x, y);
                bool nextState = rule.CalculateNextState(currentCells[gridIndex], liveNeighbours);
                nextCells[gridIndex] = nextState;
                if (nextState) localLivingCells++;
            }
        }
        Interlocked.Add(ref _nextLivingCells, localLivingCells);
        int processedCells = (endY - startY) * width;
        Interlocked.Add(ref _cellsThisSecond, processedCells);
    }

    private void TrackRates()
    {
        long now = Environment.TickCount64;
        if ((now - _lastRateTimestamp) < 1000 /* ms */) return;
        _lastRateTimestamp = now;
        Volatile.Write(ref _updatesPerSecond, _updatesThisSecond);
        Volatile.Write(ref _updatesThisSecond, 0);
        Volatile.Write(ref _cellsPerSecond, _cellsThisSecond);
        Volatile.Write(ref _cellsThisSecond, 0);
    }
}
