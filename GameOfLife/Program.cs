namespace GameOfLife;

internal class Program
{
    static void Main(string[] args)
    {
        string settingsPath = args.Length > 0 ? args[0] : "settings.json";
        GameSettings settings = GameSettings.LoadFromJson(settingsPath);
        //settings.PrintSettings();  // for debugging purposes
        //Environment.Exit(0);

        bool[] pattern = settings.UseRandomPattern
            ? PatternFactory.CreateRandom(settings.Width, settings.Height, settings.Density)
            : PatternFactory.LoadFromRleFile(settings.RlePath);
        SimulationEngine engine = new(settings);
        engine.SetCells(pattern);
        ConsoleRenderer renderer = new(engine, targetFps: settings.FpsRate);
        renderer.Start();

        while (true)
        {
            engine.UpdatePattern();
            Thread.Sleep(100);  // slow down for testing
        }
    }
}
