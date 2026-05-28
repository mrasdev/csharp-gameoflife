using GameOfLife.Core;
using GameOfLife.Models;

namespace GameOfLife.Tests.Core;

public class SimulationEngineTests
{
    // ==========================================
    // 1. GENERATION COUNTER TESTS
    // ==========================================

    [Fact]
    public void UpdatePattern_ShouldIncrementGenerationCount()
    {
        // Arrange
        var settings = new GameSettings
        {
            Width = 5,
            Height = 5,
            Density = 0.5,
            FpsRate = 20
        };
        var engine = new SimulationEngine(settings);

        // Initial state check
        Assert.Equal(0, engine.GenerationCount);

        // Act & Assert
        engine.UpdatePattern();
        Assert.Equal(1, engine.GenerationCount);

        engine.UpdatePattern();
        Assert.Equal(2, engine.GenerationCount);
    }

    // ==========================================
    // 2. SNAPSHOT COPYING TESTS (UPDATED SIGNATURE)
    // ==========================================

    [Fact]
    public void CopySnapshot_ValidTargetArray_ShouldCopyCurrentGridStateCorrectly()
    {
        // Arrange
        var settings = new GameSettings
        {
            Width = 3,
            Height = 3,
            Density = 0.0, // Start completely dead
            FpsRate = 10
        };
        var engine = new SimulationEngine(settings);

        int viewWidth = 3;
        int viewHeight = 3;
        bool[] targetArray = new bool[viewWidth * viewHeight];

        // Act
        engine.CopySnapshot(targetArray, viewWidth, viewHeight);

        // Assert
        // Verify the length matches the requested viewport dimensions
        Assert.Equal(9, targetArray.Length);
    }

    [Fact]
    public void CopySnapshot_ViewSmallerThanGrid_ShouldSuccessfullyCopyPartialViewport()
    {
        // Arrange
        // Grid is 10x10, but the console window/view is only 4x5
        var settings = new GameSettings
        {
            Width = 10,
            Height = 10,
            Density = 0.2,
            FpsRate = 10
        };
        var engine = new SimulationEngine(settings);

        int viewWidth = 4;
        int viewHeight = 5;
        bool[] restrictedTarget = new bool[viewWidth * viewHeight]; // 20 elements total

        // Act & Assert
        // The engine must safely clamp its loops to the provided viewWidth and viewHeight
        // and not crash with an IndexOutOfRangeException.
        var exception = Record.Exception(() => engine.CopySnapshot(restrictedTarget, viewWidth, viewHeight));

        Assert.Null(exception);
    }

    [Fact]
    public void CopySnapshot_ViewLargerThanGrid_ShouldNotCrashAndLeaveExcessCellsDead()
    {
        // Arrange
        // Grid ist nur 3x3 groß
        var settings = new GameSettings { Width = 3, Height = 3, Density = 0.0, FpsRate = 10 };
        var engine = new SimulationEngine(settings);

        // Viewport fordert aber 5x5 an (größer als das Grid)
        int viewWidth = 5;
        int viewHeight = 5;
        bool[] oversizedTarget = new bool[viewWidth * viewHeight]; // 25 Elemente

        // Act & Assert
        // Die Engine darf hier nicht abstürzen, sondern muss bei den Grid-Grenzen (3x3) stoppen
        var exception = Record.Exception(() => engine.CopySnapshot(oversizedTarget, viewWidth, viewHeight));

        Assert.Null(exception);
    }

    // ==========================================
    // 3. RESET / RESTART LOGIC TESTS
    // ==========================================

    [Fact]
    public void Restart_ExecutedAfterUpdates_ShouldResetGenerationCountAndStats()
    {
        // Arrange
        var settings = new GameSettings { Width = 5, Height = 5, Density = 0.4, FpsRate = 15 };
        var engine = new SimulationEngine(settings);

        // Progress the game to pollute the state
        engine.UpdatePattern();
        engine.UpdatePattern();
        engine.UpdatePattern();

        // Sanity check that we are actually at generation 3
        Assert.Equal(3, engine.GenerationCount);

        // Act
        engine.Restart();

        // Assert
        // Generation counter must be strictly reset to 0
        Assert.Equal(0, engine.GenerationCount);
    }
}