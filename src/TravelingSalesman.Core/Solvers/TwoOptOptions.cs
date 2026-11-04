namespace TravelingSalesman.Core.Solvers;

public sealed record TwoOptOptions
{
    public int MaxPasses
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            field = value;
        }
    } = 1_000;
}
