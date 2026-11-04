namespace TravelingSalesman.Core.Solvers;

public sealed record SimulatedAnnealingOptions
{
    public double InitialTemperature
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            field = value;
        }
    } = 100;

    public double FinalTemperature
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            field = value;
        }
    } = 0.01;

    public double CoolingRate
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, 1.0);
            field = value;
        }
    } = 0.995;

    public int IterationsPerTemperature
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            field = value;
        }
    } = 1_000;

    public int? Seed { get; init; }

    public static SimulatedAnnealingOptions ForCityCount(int cityCount, int? seed = null) => new()
    {
        IterationsPerTemperature = Math.Clamp(cityCount * 20, 100, 20_000),
        Seed = seed,
    };
}
