using Game.Api.Planets;

namespace Game.Api.Research;

public static class ResearchEndpoints
{
    private const string ResearchRoute = "/api/planets/{planetId:guid}/research";

    public static IEndpointRouteBuilder MapResearchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(ResearchRoute, GetResearchEndpoint.Handle)
            .WithName("GetResearch")
            .WithTags("Research")
            .WithDescription("Get your research, with costs and times for this planet's lab")
            .RequireAuthorization()
            .Produces<ResearchOverviewDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        endpoints.MapPost(ResearchRoute, StartResearchEndpoint.Handle)
            .WithName("StartResearch")
            .WithTags("Research")
            .WithDescription("Start researching the next level of a technology in this planet's lab, paying from this planet")
            .RequireAuthorization()
            .Produces<ResearchOverviewDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        endpoints.MapDelete(ResearchRoute, CancelResearchEndpoint.Handle)
            .WithName("CancelResearch")
            .WithTags("Research")
            .WithDescription("Cancel the research started on this planet and get its full cost back")
            .RequireAuthorization()
            .Produces<ResearchOverviewDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }
}