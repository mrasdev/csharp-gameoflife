using GameOfLife.CellurlarRules;

namespace GameOfLife.Tests.CellularRules;

public class HighLifeRuleTests
{
    [Fact]
    public void CalculateNextState_DeadCellWithExactlySixNeighbours_ShouldBecomeAlive()
    {
        // Arrange
        var rule = new HighLifeRule();
        bool currentState = false; // Dead cell
        int livingNeighbours = 6;

        // Act
        bool nextState = rule.CalculateNextState(currentState, livingNeighbours);

        // Assert
        Assert.True(nextState, "In HighLife, a dead cell with exactly 6 neighbors must become alive (B36 rule).");
    }

    [Fact]
    public void CalculateNextState_DeadCellWithExactlyThreeNeighbours_ShouldBecomeAlive()
    {
        // Arrange
        var rule = new HighLifeRule();
        bool currentState = false; // Dead cell
        int livingNeighbours = 3;

        // Act
        bool nextState = rule.CalculateNextState(currentState, livingNeighbours);

        // Assert
        Assert.True(nextState, "In HighLife, a dead cell with exactly 3 neighbors must also become alive.");
    }
}