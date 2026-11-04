using TravelingSalesman.Core.Benchmarking;

namespace TravelingSalesman.Specs.Drivers;

public sealed class SolverWorld
{
    private TspProblem? _problem;

    public List<City> Cities { get; } = [];

    public List<IReadOnlyList<City>> GeneratedSets { get; } = [];

    public Dictionary<string, Tour> Tours { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<Tour> RepeatedTours { get; } = [];

    public IReadOnlyList<BenchmarkResult> BenchmarkResults { get; set; } = [];

    public Tour? Tour { get; set; }

    public double? Distance { get; set; }

    public TspProblem Problem => _problem ??= new TspProblem(Cities);

    public void UseCities(IEnumerable<City> cities)
    {
        Cities.Clear();
        Cities.AddRange(cities);
        _problem = null;
    }

    public City FindCity(string name) =>
        Cities.SingleOrDefault(city => city.Name == name)
        ?? throw new InvalidOperationException($"City '{name}' has not been defined.");

    public IReadOnlyList<City> LatestGenerated => GeneratedSets.Count > 0
        ? GeneratedSets[^1]
        : throw new InvalidOperationException("No cities have been generated.");
}
