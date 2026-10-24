namespace Game.Api.Seasons;

public static class SeasonEndpoints
{
    internal const string GetCurrentSeasonRoute = "/api/seasons/current";
    private const string JoinCurrentSeasonRoute = "/api/seasons/current/join";

    public static IEndpointRouteBuilder MapSeasonEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(GetCurrentSeasonRoute, GetCurrentSeasonEndpoint.Handle)
            .WithName("GetCurrentSeason")
            .WithTags("Seasons")
            .WithDescription("Get the season that is open right now, and whether the signed-in user has joined it")
            .RequireAuthorization()
            .Produces<CurrentSeasonDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        endpoints.MapPost(JoinCurrentSeasonRoute, JoinCurrentSeasonEndpoint.Handle)
            .WithName("JoinCurrentSeason")
            .WithTags("Seasons")
            .WithDescription("Join the season that is open right now")
            .RequireAuthorization()
            .Produces<SeasonPlayerDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        return endpoints;
    }
}