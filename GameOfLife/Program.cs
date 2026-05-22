using GameOfLife.Views;
using GameOfLife.Core;
using GameOfLife.Models;

namespace GameOfLife;

internal class Program
{
    private const string DefaultPath = "settings.json";

    static void Main(string[] args)
    {
        try
        {
            string settingsPath = args.Length > 0 ? args[0] : DefaultPath;
            GameSettings settings = GameSettings.LoadFromJson(settingsPath);  // creates a new file if missing
            ShowHelpScreen(settings);

            SimulationEngine engine = new(settings);
            engine.SetCells(Pattern.GetCells(settings));
            ConsoleRenderer renderer = new(engine, settings.FpsRate);

            GameController controller = new(engine, renderer, settings.StartupMode);
            controller.Start();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor=ConsoleColor.Red;
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.ResetColor();
        }
    }

    static void ShowHelpScreen(GameSettings settings)
    {
        if (!settings.ShowHelpScreen) return;

        settings.PrintSettings();
        Console.WriteLine("\nKeyboard shortcuts:");
        Console.WriteLine("x  Quit app");
        Console.WriteLine("r  Restart with new pattern");
        Console.WriteLine("F1 Step mode, press F1 subsequently");
        Console.WriteLine("F2 Slow refreshing");
        Console.WriteLine("F3 Fast refreshing");
        Console.WriteLine("F4 Maximum performance");
        Console.Write("\nPress any key to start...");
        Console.ReadKey(intercept: true);
    }
}
