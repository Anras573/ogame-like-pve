using Microsoft.AspNetCore.Identity;

namespace Game.Api.Users;

/// <summary>
/// An account. Accounts live outside seasons; a user takes part in a season through a <see cref="Players.Player"/>.
/// </summary>
public sealed class User : IdentityUser<Guid>
{
}