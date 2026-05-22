namespace GameOfLife.Enums;

internal enum SimulationMode
{
    Step = 0,
    Slow = 1,  // 1 Hz
    Fast = 2,  // 100 Hz (but on Windows limited to ~65 Hz)
    Max = 3
}