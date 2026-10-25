using Game.Core.Units;

namespace Game.Core.Tests;

public class ShipyardQueueTests
{
    private static readonly DateTimeOffset s_start = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Advance_PartwayThroughOrder_DeliversFinishedUnitsAndKeepsProgress()
    {
        // Arrange
        ShipyardOrder[] queue = [new(UnitType.LightFighter, 5, UnitBuildSeconds: 60)];

        // Act
        var result = ShipyardQueue.Advance(queue, s_start, s_start.AddSeconds(150));

        // Assert
        Assert.Equal(2, result.Completed[UnitType.LightFighter]);
        Assert.Equal([new ShipyardOrder(UnitType.LightFighter, 3, 60)], result.Remaining);
        Assert.Equal(s_start.AddSeconds(120), result.ProgressAt); // The third unit started at 120 s
    }

    [Fact]
    public void Advance_PastSeveralOrders_DeliversThemInOrder()
    {
        // Arrange
        ShipyardOrder[] queue =
        [
            new(UnitType.RocketLauncher, 2, UnitBuildSeconds: 30),
            new(UnitType.LightFighter, 3, UnitBuildSeconds: 60),
            new(UnitType.RocketLauncher, 1, UnitBuildSeconds: 30),
        ];

        // Act
        var result = ShipyardQueue.Advance(queue, s_start, s_start.AddSeconds(60 + 120 + 10));

        // Assert
        Assert.Equal(2, result.Completed[UnitType.RocketLauncher]);
        Assert.Equal(2, result.Completed[UnitType.LightFighter]);
        Assert.Equal(
            [new ShipyardOrder(UnitType.LightFighter, 1, 60), new ShipyardOrder(UnitType.RocketLauncher, 1, 30)],
            result.Remaining);
        Assert.Equal(s_start.AddSeconds(180), result.ProgressAt);
    }

    [Fact]
    public void Advance_PastEverything_EmptiesQueue()
    {
        // Arrange
        ShipyardOrder[] queue = [new(UnitType.LightFighter, 2, UnitBuildSeconds: 60)];

        // Act
        var result = ShipyardQueue.Advance(queue, s_start, s_start.AddHours(1));

        // Assert
        Assert.Equal(2, result.Completed[UnitType.LightFighter]);
        Assert.Empty(result.Remaining);
        Assert.Null(result.ProgressAt);
    }

    [Fact]
    public void Advance_ExactlyAtUnitBoundary_DeliversThatUnit()
    {
        // Arrange
        ShipyardOrder[] queue = [new(UnitType.LightFighter, 2, UnitBuildSeconds: 60)];

        // Act
        var result = ShipyardQueue.Advance(queue, s_start, s_start.AddSeconds(60));

        // Assert
        Assert.Equal(1, result.Completed[UnitType.LightFighter]);
        Assert.Equal(s_start.AddSeconds(60), result.ProgressAt);
    }

    [Fact]
    public void Advance_BeforeProgressStarts_ChangesNothing()
    {
        // Arrange
        ShipyardOrder[] queue = [new(UnitType.LightFighter, 2, UnitBuildSeconds: 60)];

        // Act
        var result = ShipyardQueue.Advance(queue, s_start, s_start.AddSeconds(-10));

        // Assert
        Assert.Empty(result.Completed);
        Assert.Equal(queue, result.Remaining);
        Assert.Equal(s_start, result.ProgressAt);
    }
}