namespace GameOfLife;

internal class GameController
{
    private readonly SimulationEngine _engine;
    private readonly ConsoleRenderer _renderer;
    private readonly GameSettings _settings;

    private bool _isRunning;
    private SimulationMode _mode;

    private readonly ManualResetEventSlim _stepSignal = new(false);  // blocking for step mode
    private int _sleepTimeout = 0;

    public GameController(SimulationEngine engine, ConsoleRenderer renderer, GameSettings settings)
    {
        _engine = engine;
        _renderer = renderer;
        _settings = settings;
        ApplyMode(settings.StartupMode);
    }

    public void Start()
    {
        _isRunning = true;
        _renderer.Start();
        Thread updateThread = new Thread(SimulationLoop) { IsBackground = true);
        updateThread.Start();
        InputLoop();
    }

    private void SimulationLoop()
    {
        while (_isRunning)
        {
            if (_mode == SimulationMode.Step)
            {
                _stepSignal.Wait();
                _engine.UpdatePattern();
                _stepSignal.Reset();
            }
            else
            {
                _engine.UpdatePattern();
                if (_sleepTimeout > 0)
                {
                    Thread.Sleep(_sleepTimeout);
                }
            }
        }
    }

    private void InputLoop()
    {
        while (_isRunning)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.X:                         StopAndExit();                        break;
                    case ConsoleKey.R:                        Restart();                        break;
                    case ConsoleKey.F1: AppMode(SimulationMode.Step); break;
                    case ConsoleKey.F2: AppMode(SimulationMode.Slow); break;
                    case ConsoleKey.F3: AppMode(SimulationMode.Fast); break;
                    case ConsoleKey.F4: AppMode(SimulationMode.Max); break;
                }
            }
            Thread.Sleep(100);
        }
    }

    private void ApplyMode(SimulationMode mode)
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
                _sleepTimeout = 100;  // 10 Hz
                _stepSignal.Set();  // open the signal to prevent blocking
                break;
            case SimulationMode.Max:
                _sleepTimeout = 0;  // no sleep
                _stepSignal.Set();  // open the signal to prevent blocking
                break;
        }
    }

    private void StopAndExit()
    {
        _isRunning = false;
        _renderer.Stop();
        Environment.Exit(0);
    }
    private void Restart()
    {
        _engine.Restart();
    }
}
