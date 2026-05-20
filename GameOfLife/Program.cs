namespace GameOfLife;

internal class Program
{
    static void Main(string[] args)
    {
        string settingsPath = args.Length > 0 ? args[0] : "settings.json";
        GameSettings settings = GameSettings.LoadFromJson(settingsPath);
        settings.PrintSettings();  // for debugging purposes

        //SimulationEngine engine = new(settings);
        //engine.UpdatePattern();
    }
}
