using Game.Api.Players;
using Game.Api.Seasons;

namespace Game.Api.Tests;

public class PlayerTests
{
    private static readonly DateTimeOffset s_start = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Join_WhileSeasonOpen_ReturnsPlayer()
    {
        // Arrange
        var season = Season.Create("Season 1", s_start, s_start.AddDays(84));
        var userId = Guid.NewGuid();
        var now = s_start.AddHours(1);

        // Act
        var player = Player.Join(season, userId, now);

        // Assert
        Assert.NotNull(player);
        Assert.Equal(userId, player.UserId);
        Assert.Equal(season.Id, player.SeasonId);
        Assert.Equal(now, player.JoinedAt);
    }

    [Fact]
    public void Join_BeforeSeasonStarts_ReturnsNull()
    {
        // Arrange
        var season = Season.Create("Season 1", s_start, s_start.AddDays(84));

        // Act
        var player = Player.Join(season, Guid.NewGuid(), s_start.AddMinutes(-1));

        // Assert
        Assert.Null(player);
    }

    [Fact]
    public void Join_AfterSeasonEnds_ReturnsNull()
    {
        // Arrange
        var season = Season.Create("Season 1", s_start, s_start.AddDays(84));

        // Act
        var player = Player.Join(season, Guid.NewGuid(), s_start.AddDays(85));

        // Assert
        Assert.Null(player);
    }

    [Fact]
    public void Join_WithNullSeason_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => Player.Join(null!, Guid.NewGuid(), s_start));
    }

    [Fact]
    public void Join_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var season = Season.Create("Season 1", s_start, s_start.AddDays(84));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Player.Join(season, Guid.Empty, s_start));
    }
}