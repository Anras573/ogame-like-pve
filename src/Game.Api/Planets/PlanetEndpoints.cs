namespace Game.Api.Planets;

public static class PlanetEndpoints
{
    private const string PlanetsRoute = "/api/planets";
    private const string PlanetRoute = "/api/planets/{planetId:guid}";
    private const string ConstructionRoute = "/api/planets/{planetId:guid}/construction";
    private const string ShipyardRoute = "/api/planets/{planetId:guid}/shipyard";

    public static IEndpointRouteBuilder MapPlanetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(PlanetsRoute, GetPlanetsEndpoint.Handle)
            .WithName("GetPlanets")
            .WithTags("Planets")
            .WithDescription("List the signed-in player's planets in the current season")
            .RequireAuthorization()
            .Produces<List<PlanetSummaryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        endpoints.MapGet(PlanetRoute, GetPlanetEndpoint.Handle)
            .WithName("GetPlanet")
            .WithTags("Planets")
            .WithDescription("Get one of your planets, brought up to date")
            .RequireAuthorization()
            .Produces<PlanetDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        endpoints.MapPost(ConstructionRoute, StartConstructionEndpoint.Handle)
            .WithName("StartConstruction")
            .WithTags("Planets")
            .WithDescription("Start upgrading a building by one level, paying its cost now")
            .RequireAuthorization()
            .Produces<PlanetDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        endpoints.MapDelete(ConstructionRoute, CancelConstructionEndpoint.Handle)
            .WithName("CancelConstruction")
            .WithTags("Planets")
            .WithDescription("Cancel the construction in progress and get its full cost back")
            .RequireAuthorization()
            .Produces<PlanetDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        endpoints.MapPost(ShipyardRoute, EnqueueUnitsEndpoint.Handle)
            .WithName("EnqueueUnits")
            .WithTags("Planets")
            .WithDescription("Order ships or defenses from this planet's shipyard, paying for all of them now")
            .RequireAuthorization()
            .Produces<PlanetDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }
}