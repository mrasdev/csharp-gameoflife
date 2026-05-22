namespace GameOfLife.Enums;

public enum SimulationMode
{
    Step,
    Slow,  // 1 Hz
    Fast,  // 100 Hz (but on Windows limited to ~65 Hz)
    Max
}