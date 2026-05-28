using GameOfLife.CellurlarRules;

namespace GameOfLife.Tests.CellularRules;

public class ConwayRuleTests
{
    [Fact]
    public void CalculateNextState_DeadCellWithExactlyThreeNeighbours_ShouldBecomeAlive()
    {
        // Arrange
        var rule = new ConwayRule();
        bool currentState = false; // Dead cell
        int livingNeighbours = 3;

        // Act
        bool nextState = rule.CalculateNextState(currentState, livingNeighbours);

        // Assert
        Assert.True(nextState, "According to Conway's rules, a dead cell with exactly 3 neighbors must become alive (Birth).");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void CalculateNextState_LivingCellWithTwoOrThreeNeighbours_ShouldSurvive(int neighbours)
    {
        // Arrange
        var rule = new ConwayRule();
        bool currentState = true; // Living cell

        // Act
        bool nextState = rule.CalculateNextState(currentState, neighbours);

        // Assert
        Assert.True(nextState, $"A living cell with {neighbours} neighbors should survive.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void CalculateNextState_LivingCellWithTooFewNeighbours_ShouldDieOfUnderpopulation(int neighbours)
    {
        // Arrange
        var rule = new ConwayRule();
        bool currentState = true; // Living cell

        // Act
        bool nextState = rule.CalculateNextState(currentState, neighbours);

        // Assert
        Assert.False(nextState, $"A living cell with {neighbours} neighbors must die due to underpopulation/solitude.");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void CalculateNextState_LivingCellWithTooManyNeighbours_ShouldDieOfOverpopulation(int neighbours)
    {
        // Arrange
        var rule = new ConwayRule();
        bool currentState = true; // Living cell

        // Act
        bool nextState = rule.CalculateNextState(currentState, neighbours);

        // Assert
        Assert.False(nextState, $"A living cell with {neighbours} neighbors must die due to overpopulation.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    // 3 is omitted here because it triggers a birth
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)] // CRITICAL: Verification that Conway does NOT behave like HighLife here
    [InlineData(7)]
    [InlineData(8)]
    public void CalculateNextState_DeadCellWithIncorrectNeighbourCount_ShouldRemainDead(int neighbours)
    {
        // Arrange
        var rule = new ConwayRule();
        bool currentState = false; // Dead cell

        // Act
        bool nextState = rule.CalculateNextState(currentState, neighbours);

        // Assert
        Assert.False(nextState, $"A dead cell with {neighbours} neighbors must remain dead.");
    }
}