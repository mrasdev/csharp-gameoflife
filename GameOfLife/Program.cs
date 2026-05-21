namespace GameOfLife;

internal class Program
{
    static void Main(string[] args)
    {
        string settingsPath = args.Length > 0 ? args[0] : "settings.json";
        GameSettings settings = GameSettings.LoadFromJson(settingsPath);
        //settings.PrintSettings();  // for debugging purposes
        //Environment.Exit(0);

        bool[] cells = Pattern.GetCells(settings);
        SimulationEngine engine = new(settings);
        engine.SetCells(cells);
        ConsoleRenderer renderer = new(engine, targetFps: settings.FpsRate);

        GameController controller = new(engine, renderer, settings);
        controller.Start();
    }
}
