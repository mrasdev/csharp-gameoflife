using GameOfLife.Views;
using GameOfLife.Core;
using GameOfLife.Models;

namespace GameOfLife;

internal class Program
{
    static void Main(string[] args)
    {
        string settingsPath = args.Length > 0 ? args[0] : "settings.json";
        GameSettings settings = GameSettings.LoadFromJson(settingsPath);
        ShowHelpScreen(settings);

        bool[] cells = Pattern.GetCells(settings);
        SimulationEngine engine = new(settings);
        engine.SetCells(cells);
        ConsoleRenderer renderer = new(engine, targetFps: settings.FpsRate);

        GameController controller = new(engine, renderer, settings.StartupMode);
        controller.Start();
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
        Console.ReadKey();
    }
}
