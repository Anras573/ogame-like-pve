using Game.Api.Users;

namespace Game.Api.Auth;

public static class AuthEndpoints
{
    public const string RateLimitPolicy = "auth";

    private const string AuthGroup = "/api/auth";
    private const string CurrentUserEndpoint = "/me";
    private const string LogoutEndpointRoute = "/logout";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(AuthGroup)
            .WithTags("Auth")
            .RequireRateLimiting(RateLimitPolicy);

        // Built-in Identity endpoints: /register, /login (use ?useCookies=true from the browser), /refresh, /manage/...
        group.MapIdentityApi<User>();

        group.MapGet(CurrentUserEndpoint, GetCurrentUserEndpoint.Handle)
            .WithName("GetCurrentUser")
            .WithDescription("Get the signed-in user")
            .RequireAuthorization()
            .Produces<CurrentUserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost(LogoutEndpointRoute, LogoutEndpoint.Handle)
            .WithName("Logout")
            .WithDescription("Sign out and clear the auth cookie")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }
}