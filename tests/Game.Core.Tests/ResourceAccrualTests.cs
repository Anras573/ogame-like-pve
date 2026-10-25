using Game.Core.Economy;
using Game.Core.Resources;

namespace Game.Core.Tests;

public class ResourceAccrualTests
{
    private static readonly ResourceAmounts s_capacity = new(10_000, 10_000, 10_000);

    [Fact]
    public void Accrue_ForTwoHours_AddsTwoHoursOfProduction()
    {
        // Arrange
        var current = new ResourceAmounts(500, 500, 0);
        var hourly = new ResourceAmounts(30, 15, 5);

        // Act
        var result = ResourceAccrual.Accrue(current, hourly, s_capacity, TimeSpan.FromHours(2));

        // Assert
        Assert.Equal(new ResourceAmounts(560, 530, 10), result);
    }

    [Fact]
    public void Accrue_PastCapacity_StopsAtCapacity()
    {
        // Arrange
        var current = new ResourceAmounts(9_990, 0, 0);
        var hourly = new ResourceAmounts(100, 0, 0);

        // Act
        var result = ResourceAccrual.Accrue(current, hourly, s_capacity, TimeSpan.FromHours(1));

        // Assert
        Assert.Equal(10_000, result.Metal);
    }

    [Fact]
    public void Accrue_AboveCapacity_KeepsAmountWithoutProducing()
    {
        // Arrange
        var current = new ResourceAmounts(12_000, 0, 0);
        var hourly = new ResourceAmounts(100, 0, 0);

        // Act
        var result = ResourceAccrual.Accrue(current, hourly, s_capacity, TimeSpan.FromHours(1));

        // Assert
        Assert.Equal(12_000, result.Metal);
    }

    [Fact]
    public void Accrue_WithNoElapsedTime_ReturnsCurrent()
    {
        // Arrange
        var current = new ResourceAmounts(500, 500, 0);

        // Act
        var result = ResourceAccrual.Accrue(current, new ResourceAmounts(30, 15, 0), s_capacity, TimeSpan.Zero);

        // Assert
        Assert.Equal(current, result);
    }
}