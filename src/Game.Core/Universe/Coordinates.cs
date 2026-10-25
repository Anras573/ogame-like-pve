namespace Game.Core.Universe;

/// <summary>
/// A position in the universe, written galaxy:system:position (e.g. 1:42:8).
/// </summary>
public sealed record Coordinates(int Galaxy, int System, int Position)
{
    public override string ToString() => $"{Galaxy}:{System}:{Position}";
}