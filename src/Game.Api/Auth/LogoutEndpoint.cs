using Game.Api.Users;
using Microsoft.AspNetCore.Identity;

namespace Game.Api.Auth;

public sealed class LogoutEndpoint
{
    public static async Task<IResult> Handle(SignInManager<User> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.NoContent();
    }
}