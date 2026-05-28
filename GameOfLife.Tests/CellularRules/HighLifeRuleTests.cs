using GameOfLife.CellurlarRules;

namespace GameOfLife.Tests.CellularRules;

public class HighLifeRuleTests
{
    [Theory]
    [InlineData(3)]
    [InlineData(6)] // The defining difference to Conway's Game of Life
    public void CalculateNextState_DeadCellWithThreeOrSixNeighbours_ShouldBecomeAlive(int neighbours)
    {
        // Arrange
        var rule = new HighLifeRule();
        bool currentState = false; // Dead cell

        // Act
        bool nextState = rule.CalculateNextState(currentState, neighbours);

        // Assert
        Assert.True(nextState, $"In HighLife, a dead cell with exactly {neighbours} neighbors must become alive (B36 rule).");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    // 3 is omitted (triggers birth)
    [InlineData(4)]
    [InlineData(5)]
    // 6 is omitted (triggers birth)
    [InlineData(7)]
    [InlineData(8)]
    public void CalculateNextState_DeadCellWithIncorrectNeighbourCount_ShouldRemainDead(int neighbours)
    {
        // Arrange
        var rule = new HighLifeRule();
        bool currentState = false; // Dead cell

        // Act
        bool nextState = rule.CalculateNextState(currentState, neighbours);

        // Assert
        Assert.False(nextState, $"A dead cell with {neighbours} neighbors must remain dead under HighLife rules.");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void CalculateNextState_LivingCellWithTwoOrThreeNeighbours_ShouldSurvive(int neighbours)
    {
        // Arrange
        var rule = new HighLifeRule();
        bool currentState = true; // Living cell

        // Act
        bool nextState = rule.CalculateNextState(currentState, neighbours);

        // Assert
        Assert.True(nextState, $"A living cell with {neighbours} neighbors should survive in HighLife (S23 rule).");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void CalculateNextState_LivingCellWithTooFewNeighbours_ShouldDieOfUnderpopulation(int neighbours)
    {
        // Arrange
        var rule = new HighLifeRule();
        bool currentState = true; // Living cell

        // Act
        bool nextState = rule.CalculateNextState(currentState, neighbours);

        // Assert
        Assert.False(nextState, $"A living cell with {neighbours} neighbors must die due to underpopulation.");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)] // Even though 6 causes birth for dead cells, a living cell with 6 neighbors dies of overcrowding!
    [InlineData(7)]
    [InlineData(8)]
    public void CalculateNextState_LivingCellWithTooManyNeighbours_ShouldDieOfOverpopulation(int neighbours)
    {
        // Arrange
        var rule = new HighLifeRule();
        bool currentState = true; // Living cell

        // Act
        bool nextState = rule.CalculateNextState(currentState, neighbours);

        // Assert
        Assert.False(nextState, $"A living cell with {neighbours} neighbors must die due to overpopulation.");
    }
}