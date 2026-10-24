namespace Game.Core.Resources;

/// <summary>
/// An amount of each storable resource. Amounts are fractional while they accrue; show them floored.
/// </summary>
public readonly record struct ResourceAmounts(double Metal, double Crystal, double Deuterium)
{
    public static ResourceAmounts Zero { get; } = new(0, 0, 0);

    public static ResourceAmounts operator +(ResourceAmounts left, ResourceAmounts right)
        => new(left.Metal + right.Metal, left.Crystal + right.Crystal, left.Deuterium + right.Deuterium);

    public static ResourceAmounts operator -(ResourceAmounts left, ResourceAmounts right)
        => new(left.Metal - right.Metal, left.Crystal - right.Crystal, left.Deuterium - right.Deuterium);

    public static ResourceAmounts operator *(ResourceAmounts amounts, double factor)
        => new(amounts.Metal * factor, amounts.Crystal * factor, amounts.Deuterium * factor);

    /// <summary>
    /// True when every resource is at least the corresponding amount in <paramref name="other"/>.
    /// </summary>
    public bool Covers(ResourceAmounts other)
    {
        return Metal >= other.Metal && Crystal >= other.Crystal && Deuterium >= other.Deuterium;
    }

    public ResourceAmounts Floor()
    {
        return new(Math.Floor(Metal), Math.Floor(Crystal), Math.Floor(Deuterium));
    }
}