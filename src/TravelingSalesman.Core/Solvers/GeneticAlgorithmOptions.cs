namespace TravelingSalesman.Core.Solvers;

public sealed record GeneticAlgorithmOptions
{
    public int PopulationSize
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 2);
            field = value;
        }
    } = 100;

    public int Generations
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            field = value;
        }
    } = 500;

    public double MutationRate
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 1.0);
            field = value;
        }
    } = 0.2;

    public double EliteFraction
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, 1.0);
            field = value;
        }
    } = 0.1;

    public int TournamentSize
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            field = value;
        }
    } = 5;

    public int StagnationLimit
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            field = value;
        }
    } = 100;

    public int? Seed { get; init; }

    public static GeneticAlgorithmOptions ForCityCount(int cityCount, int? seed = null) => new()
    {
        PopulationSize = Math.Clamp(cityCount * 2, 50, 200),
        Generations = Math.Clamp(cityCount * 20, 100, 2_000),
        Seed = seed,
    };
}
