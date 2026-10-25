using Game.Core.Universe;

namespace Game.Core.Tests;

public class PlanetPlacementTests
{
    [Fact]
    public void HomeSearchOrder_Always_VisitsEverySystemOnce()
    {
        // Arrange
        var settings = new UniverseSettings(galaxies: 3, systemsPerGalaxy: 7, economySpeed: 1);

        // Act
        var systems = PlanetPlacement.HomeSearchOrder(settings, new Random(1234)).ToList();

        // Assert
        Assert.Equal(21, systems.Count);
        Assert.Equal(21, systems.Distinct().Count());
        Assert.All(systems, s => Assert.True(settings.Contains(new Coordinates(s.Galaxy, s.System, 1))));
    }

    [Fact]
    public void HomeSearchOrder_DifferentSeeds_StartInDifferentPlaces()
    {
        // Arrange
        var settings = UniverseSettings.Default;

        // Act
        var starts = Enumerable.Range(0, 20)
            .Select(seed => PlanetPlacement.HomeSearchOrder(settings, new Random(seed)).First())
            .Distinct()
            .Count();

        // Assert
        Assert.True(starts > 1);
    }

    [Fact]
    public void PickFreeHomePosition_WithSomeTaken_ReturnsFreeTemperatePosition()
    {
        // Arrange
        int[] occupied = [4, 5, 6, 7, 8, 9, 10, 11];

        // Act
        var position = PlanetPlacement.PickFreeHomePosition(occupied, new Random(1234));

        // Assert
        Assert.Equal(12, position);
    }

    [Fact]
    public void PickFreeHomePosition_WithAllHomePositionsTaken_ReturnsNull()
    {
        // Arrange
        int[] occupied = [1, 4, 5, 6, 7, 8, 9, 10, 11, 12];

        // Act
        var position = PlanetPlacement.PickFreeHomePosition(occupied, new Random(1234));

        // Assert
        Assert.Null(position);
    }

    [Fact]
    public void RollMaxTemperature_InnerOrbit_IsHotterThanOuterOrbit()
    {
        // Arrange
        var random = new Random(1234);

        // Act
        var inner = PlanetPlacement.RollMaxTemperature(1, random);
        var outer = PlanetPlacement.RollMaxTemperature(15, random);

        // Assert
        Assert.InRange(inner, 220, 240);
        Assert.InRange(outer, -130, -110);
    }

    [Fact]
    public void RollMaxTemperature_PositionOutsideSystem_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => PlanetPlacement.RollMaxTemperature(16, new Random()));
    }
}