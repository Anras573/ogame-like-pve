using System.Security.Claims;
using Game.Api.Users;
using Microsoft.AspNetCore.Identity;

namespace Game.Api.Auth;

public sealed class GetCurrentUserEndpoint
{
    public static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        UserManager<User> userManager)
    {
        // The cookie may outlive the account, so look the user up rather than trusting claims alone
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new CurrentUserDto(user.Id, user.Email ?? string.Empty));
    }
}

public record CurrentUserDto(Guid Id, string Email);