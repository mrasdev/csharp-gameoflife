namespace GameOfLife;

internal class GameController
{
    private readonly SimulationEngine _engine;
    private readonly ConsoleRenderer _renderer;
    private readonly GameSettings _settings;
    private bool _isRunning;
    private Thread? _updateThread;

    public GameController(SimulationEngine engine, ConsoleRenderer renderer, GameSettings settings)
    {
        _engine = engine;
        _renderer = renderer;
        _settings = settings;
    }

    public void Start()
    {
        _isRunning = true;
        _renderer.Start();
        _updateThread = new Thread(SimulationLoop);
        _updateThread.IsBackground = true;
        _updateThread.Start();
        InputLoop();
    }

    private void SimulationLoop()
    {
        while (_isRunning)
        {
            _engine.UpdatePattern();
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
                    case ConsoleKey.X:
                        StopAndExit();
                        break;
                    case (ConsoleKey.R):
                        Restart();
                        break;
                }
            }
            Thread.Sleep(100);
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
