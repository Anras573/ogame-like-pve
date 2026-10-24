using Game.Api.Infrastructure.Repositories;
using Game.Api.Seasons.Specifications;

namespace Game.Api.Seasons;

public static class CurrentSeason
{
    /// <summary>
    /// The season new players join: the most recently started season that is open right now.
    /// </summary>
    public static async Task<Season?> FindAsync(
        IRepository<Season> seasonRepository,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var openSeasons = await seasonRepository.ListAsync(
            new SeasonsOpenAtSpecification(timeProvider.GetUtcNow()),
            cancellationToken);

        return openSeasons.MaxBy(s => s.StartsAt);
    }
}