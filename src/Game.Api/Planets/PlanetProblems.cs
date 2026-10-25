namespace Game.Api.Planets;

/// <summary>
/// Problem responses for planet actions that can't happen in the planet's current state.
/// </summary>
public static class PlanetProblems
{
    public static IResult AlreadyBuilding() => Conflict("Already building", "This planet is already constructing a building.");

    public static IResult NotEnoughResources() => Conflict("Not enough resources", "This planet can't afford that upgrade yet.");

    public static IResult NothingToCancel() => Conflict("Nothing to cancel", "This planet isn't constructing anything.");

    public static IResult SeasonClosed() => Conflict("Season closed", "This season isn't open, so nothing can be built.");

    public static IResult ChangedConcurrently() => Conflict("Planet changed", "The planet changed while you were acting on it. Please try again.");

    private static IResult Conflict(string title, string detail)
    {
        return Results.Problem(statusCode: StatusCodes.Status409Conflict, title: title, detail: detail);
    }
}