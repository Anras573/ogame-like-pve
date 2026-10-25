using Game.Core.Universe;

namespace Game.Core.Tests;

public class UniverseSettingsTests
{
    [Fact]
    public void Contains_CoordinatesInsideBounds_ReturnsTrue()
    {
        // Arrange
        var settings = new UniverseSettings(galaxies: 2, systemsPerGalaxy: 10, economySpeed: 1);

        // Act
        var result = settings.Contains(new Coordinates(2, 10, 15));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Contains_SystemOutsideBounds_ReturnsFalse()
    {
        // Arrange
        var settings = new UniverseSettings(galaxies: 2, systemsPerGalaxy: 10, economySpeed: 1);

        // Act
        var result = settings.Contains(new Coordinates(1, 11, 5));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Constructor_WithZeroSpeed_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new UniverseSettings(1, 1, 0));
    }
}