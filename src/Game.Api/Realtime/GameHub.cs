using Game.Core.Buildings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Game.Api.Realtime;

/// <summary>
/// Pushes game events to signed-in players. Clients only listen; all actions go through the HTTP API.
/// </summary>
[Authorize]
public sealed class GameHub : Hub<IGameClient>
{
    public const string Route = "/hubs/game";
}

public interface IGameClient
{
    Task BuildingCompleted(BuildingCompletedMessage message);
}

public record BuildingCompletedMessage(Guid PlanetId, BuildingType Building, int Level);