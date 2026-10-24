using Game.Api.Seasons;

namespace Game.Api.Tests;

public class SeasonTests
{
    private static readonly DateTimeOffset s_start = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidArguments_SetsProperties()
    {
        // Arrange
        var end = s_start.AddDays(84);

        // Act
        var season = Season.Create("Season 1", s_start, end);

        // Assert
        Assert.Equal("Season 1", season.Name);
        Assert.Equal(s_start, season.StartsAt);
        Assert.Equal(end, season.EndsAt);
        Assert.NotEqual(Guid.Empty, season.Id.Value);
    }

    [Fact]
    public void Create_WithNonUtcOffset_StoresUtc()
    {
        // Arrange
        var localStart = new DateTimeOffset(2026, 11, 1, 14, 0, 0, TimeSpan.FromHours(2));

        // Act
        var season = Season.Create("Season 1", localStart, localStart.AddDays(1));

        // Assert
        Assert.Equal(TimeSpan.Zero, season.StartsAt.Offset);
        Assert.Equal(localStart, season.StartsAt);
    }

    [Fact]
    public void Create_WithEndBeforeStart_ThrowsArgumentException()
    {
        // Arrange
        var end = s_start.AddDays(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Season.Create("Season 1", s_start, end));
    }

    [Fact]
    public void Create_WithEndEqualToStart_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => Season.Create("Season 1", s_start, s_start));
    }

    [Fact]
    public void Create_WithBlankName_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => Season.Create("  ", s_start, s_start.AddDays(1)));
    }

    [Fact]
    public void Create_WithTooLongName_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var name = new string('x', Season.MaxNameLength + 1);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Season.Create(name, s_start, s_start.AddDays(1)));
    }

    [Fact]
    public void IsOpenAt_AtStart_ReturnsTrue()
    {
        // Arrange
        var season = Season.Create("Season 1", s_start, s_start.AddDays(1));

        // Act
        var result = season.IsOpenAt(s_start);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOpenAt_BeforeStart_ReturnsFalse()
    {
        // Arrange
        var season = Season.Create("Season 1", s_start, s_start.AddDays(1));

        // Act
        var result = season.IsOpenAt(s_start.AddTicks(-1));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOpenAt_AtEnd_ReturnsFalse()
    {
        // Arrange
        var season = Season.Create("Season 1", s_start, s_start.AddDays(1));

        // Act
        var result = season.IsOpenAt(s_start.AddDays(1));

        // Assert
        Assert.False(result);
    }
}