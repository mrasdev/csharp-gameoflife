using System.Text;

namespace GameOfLife;

internal class ConsoleRenderer
{
    private readonly SimulationEngine _engine;
    private readonly int _targetFps;
    private bool _isRunning;
    private Thread? _renderThread;

    // local buffer to avoid flickering
    private bool[] _displayBuffer = Array.Empty<bool>();  // safe zero alloc placeholder until window size is known
    private int _currentWidth;  // display width
    private int _currentHeight;  // display height

    public ConsoleRenderer(SimulationEngine engine, int targetFps = 5)
    {
        _engine = engine;
        _targetFps = targetFps;
        Console.CursorVisible = false;
    }

    public void Start()
    {
        _isRunning = true;
        _renderThread = new Thread(RenderLoop)
        {
            Name = "ConsoleRenderThread",
            IsBackground = true
        };
        _renderThread.Start();
    }

    public void Stop()
    {
        _isRunning = false;
        _renderThread?.Join();
        Console.CursorVisible = true;
    }

    private void RenderLoop()
    {
        int interval = 1000 / _targetFps;
        while (_isRunning)
        {
            UpdateBufferSize();  // check if console window size has changed
            _engine.CopySnapshot(_displayBuffer, _currentWidth, _currentHeight);
            WriteScreen();
            Thread.Sleep(interval);
        }
    }

    private void UpdateBufferSize()
    {
        int windowWidth = Console.WindowWidth;
        int windowHeight = Console.WindowHeight - 1;  // bottom line shows status infos
        if (windowWidth != _currentWidth || windowHeight != _currentHeight)
        {
            _currentWidth = windowWidth;
            _currentHeight = windowHeight;
            _displayBuffer = new bool[_currentWidth * _currentHeight];
            Console.Clear();
        }
    }

    private StringBuilder DrawFrame()
    {
        StringBuilder sb = new(_currentWidth * _currentHeight);
        for (int y = 0; y < _currentHeight; y++)
        {
            int rowOffset = y * _currentWidth;
            for (int x = 0; x < _currentWidth; x++)
            {
                bool isAlive = _displayBuffer[rowOffset + x];
                sb.Append(isAlive ? '█' : ' ');
            }
            sb.AppendLine();
        }
        return sb;
    }

    private void WriteScreen()
    {
        Console.SetCursorPosition(0, 0);
        StringBuilder sb = DrawFrame();
        sb.AppendLine();
        Console.Write(sb.ToString());
        // TODO: Add status line
    }
}
