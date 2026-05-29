using GameOfLife.Core;
using GameOfLife.Enums;
using GameOfLife.Models;
using System.Text;

namespace GameOfLife.Views;

internal class ConsoleRenderer
{
    public SimulationMode CurrentMode { get; set; }  // within the class, property will only be read for display

    private readonly SimulationEngine _engine;
    private readonly int _targetFps;
    private bool _isRunning;
    private Thread? _renderThread;

    // local buffer to avoid flickering
    private bool[] _cellsBuffer = [];  // safe zero alloc placeholder until window size is known
    private readonly StringBuilder _screenBuffer = new();
    private int _currentWidth;  // grid = display width
    private int _currentHeight;  // grid height = display height - 1 (because of additional status line)

    public ConsoleRenderer(SimulationEngine engine, int targetFps = 5)
    {
        _engine = engine;
        _targetFps = targetFps;
    }

    public void Start()
    {
        _isRunning = true;
        _renderThread = new Thread(RenderLoop)
        {
            Name = "ConsoleRenderThread",
            IsBackground = true
        };
        Console.CursorVisible = false;
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
            UpdateScreenBufferSize();  // check if console window size has changed
            _engine.CopySnapshot(_cellsBuffer, _currentWidth, _currentHeight);
            WriteScreen();
            Thread.Sleep(interval);
        }
    }

    private void UpdateScreenBufferSize()
    {
        int gridWidth = Math.Max(1, Console.WindowWidth);
        int gridHeight = Math.Max(1, Console.WindowHeight - 1);  // bottom line shows status infos
        if (gridWidth == _currentWidth && gridHeight == _currentHeight) return;

        _currentWidth = gridWidth;
        _currentHeight = gridHeight;
        _cellsBuffer = new bool[_currentWidth * _currentHeight];
        int screenbufferSize = _cellsBuffer.Length + (_currentHeight * Environment.NewLine.Length)
            + _currentWidth;  // additional status line
        _screenBuffer.Clear();  // avoid ArgumentOutOfRangeException if _screenBuffer.Length > screenBufferSize
        _screenBuffer.Capacity = screenbufferSize;
        Console.Clear();
    }

    private void DrawCellsInBuffer()
    {
        _screenBuffer.Clear();
        for (int y = 0; y < _currentHeight; y++)
        {
            int rowOffset = y * _currentWidth;
            for (int x = 0; x < _currentWidth; x++)
            {
                bool isAlive = _cellsBuffer[rowOffset + x];
                _screenBuffer.Append(isAlive ? '█' : ' ');
            }
            _screenBuffer.AppendLine();
        }
    }

    private void WriteScreen()
    {
        SimulationStats stats = _engine.GetStats();
        string statsLine = $"Gen {stats.GenerationCount,10:n0} | ";
        statsLine += $"Alive {stats.LivingCells,9:n0} | ";
        statsLine += $"Grids {stats.UpdatesPerSecond,6:n0} /s | ";
        statsLine += $"Checks {stats.NeighbourChecksPerSecond,13:n0} /s | ";
        statsLine += $"Disp {_currentWidth,3} x {_currentHeight,3} | ";
        statsLine += $"Grid {_engine.Width,4} x {_engine.Height,4} | ";
        statsLine += $"Mode: {CurrentMode,-10}";
        if (statsLine.Length > _currentWidth) statsLine = statsLine[.._currentWidth];
        DrawCellsInBuffer();
        _screenBuffer.Append(statsLine);
        Console.SetCursorPosition(0, 0);
        Console.Write(_screenBuffer.ToString());
    }
}
