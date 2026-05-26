using GameOfLife.Core;
using GameOfLife.Enums;
using GameOfLife.Views;

namespace GameOfLife;

internal class GameController : IDisposable
{
    private readonly SimulationEngine _engine;
    private readonly ConsoleRenderer _renderer;

    private volatile bool _isRunning;

    private readonly Lock _stateLock = new();
    private SimulationMode _mode;
    private int _sleepTimeout = 0;

    private readonly ManualResetEventSlim _stepSignal = new(false);  // blocking for step mode

    public GameController(SimulationEngine engine, ConsoleRenderer renderer, SimulationMode startup)
    {
        _engine = engine;
        _renderer = renderer;
        ApplyMode(startup);
    }

    public void Start()
    {
        _isRunning = true;
        _renderer.Start();
        Thread updateThread = new(SimulationLoop) { IsBackground = true };
        updateThread.Start();
        InputLoop();  // this (intenionally) blocks the main thread
    }

    private void SimulationLoop()
    {
        while (_isRunning)
        {
            SimulationMode currentMode;
            int currentTimeout;
            lock (_stateLock)  // we take a snapshot, i.e. only lock the reading not the whole process
            {
                currentMode = _mode;
                currentTimeout = _sleepTimeout;
            }

            if (currentMode == SimulationMode.Step)
            {
                _stepSignal.Wait();
                if (!_isRunning) break;  // check again in case _isRunning changed while we were waiting
                _engine.UpdatePattern();
                _stepSignal.Reset();
            }
            else
            {
                _engine.UpdatePattern();
                if (currentTimeout > 0)
                {
                    Thread.Sleep(currentTimeout);
                }
            }
        }
    }

    private void InputLoop()
    {
        while (_isRunning)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);  // blocks until a key is pressed
            switch (keyInfo.Key)
            {
                case ConsoleKey.X: StopAndExit(); break;
                case ConsoleKey.R: Restart(); break;
                case ConsoleKey.F1: ApplyMode(SimulationMode.Step); break;
                case ConsoleKey.F2: ApplyMode(SimulationMode.Slow); break;
                case ConsoleKey.F3: ApplyMode(SimulationMode.Fast); break;
                case ConsoleKey.F4: ApplyMode(SimulationMode.Max); break;
            }
        }
    }

    private void ApplyMode(SimulationMode mode)
    {
        _renderer.CurrentMode = mode;

        lock (_stateLock)  // make the whole thing atomic for the outer world because we change values
        {
            _mode = mode;
            switch (mode)
            {
                case SimulationMode.Step:
                    _sleepTimeout = 0;
                    _stepSignal.Set();  // F1 keypress triggers a new step
                    break;
                case SimulationMode.Slow:
                    _sleepTimeout = 1000;  // 1 Hz
                    _stepSignal.Set();  // open the signal to prevent blocking
                    break;
                case SimulationMode.Fast:
                    _sleepTimeout = 10;  // could be rounded up to 16..31 ms by Windows
                    _stepSignal.Set();  // open the signal to prevent blocking
                    break;
                case SimulationMode.Max:
                    _sleepTimeout = 0;  // no sleep
                    _stepSignal.Set();  // open the signal to prevent blocking
                    break;
            }
        }
    }

    private void StopAndExit()
    {
        _isRunning = false;
        _stepSignal.Set();  // wake up the simulation thread if it's trapped in Step mode
        _renderer.Stop();
        Environment.Exit(0);
    }

    private void Restart()
    {
        _engine.Restart();
    }

    public void Dispose()
    {
        _stepSignal?.Dispose();
    }
}
