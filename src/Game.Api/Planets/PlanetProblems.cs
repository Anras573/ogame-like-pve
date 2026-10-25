namespace Game.Api.Planets;

/// <summary>
/// Problem responses for planet actions that can't happen in the planet's current state.
/// </summary>
public static class PlanetProblems
{
    public static IResult AlreadyBuilding() => Conflict("Already building", "This planet is already constructing a building.");

    public static IResult NotEnoughResources() => Conflict("Not enough resources", "This planet can't afford that upgrade yet.");

    public static IResult RequirementsNotMet() => Conflict("Requirements not met", "Its prerequisites aren't in place yet.");

    public static IResult AlreadyResearching() => Conflict("Already researching", "You are already researching something.");

    public static IResult NoResearchToCancel() => Conflict("Nothing to cancel", "You aren't researching anything.");

    public static IResult ResearchOnOtherPlanet() => Conflict("Research runs elsewhere", "The research in progress was started on another planet.");

    public static IResult InvalidQuantity() => Conflict("Invalid quantity", $"Order between 1 and {Game.Core.Units.ShipyardQueue.MaxQuantityPerOrder:N0} units.");

    public static IResult ShipyardQueueFull() => Conflict("Shipyard queue full", $"The shipyard takes at most {Game.Core.Units.ShipyardQueue.MaxOrders} orders at a time.");

    public static IResult UnitLimitReached() => Conflict("Limit reached", "A planet can only have one of these.");

    public static IResult NothingToCancel() => Conflict("Nothing to cancel", "This planet isn't constructing anything.");

    public static IResult SeasonClosed() => Conflict("Season closed", "This season isn't open, so nothing can be built.");

    public static IResult ChangedConcurrently() => Conflict("Planet changed", "The planet changed while you were acting on it. Please try again.");

    private static IResult Conflict(string title, string detail)
    {
        return Results.Problem(statusCode: StatusCodes.Status409Conflict, title: title, detail: detail);
    }
}