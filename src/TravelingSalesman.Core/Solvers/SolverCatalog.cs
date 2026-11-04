using System.Diagnostics.CodeAnalysis;

namespace TravelingSalesman.Core.Solvers;

public static class SolverCatalog
{
    public static SolverDescriptor NearestNeighbor { get; } = new(
        "nearest-neighbor",
        NearestNeighborSolver.DisplayName,
        "O(n²)",
        "Greedy construction: always travel to the closest unvisited city.",
        static (_, _) => new NearestNeighborSolver());

    public static SolverDescriptor TwoOpt { get; } = new(
        "two-opt",
        TwoOptSolver.DisplayName,
        "O(n²) per pass",
        "Local search that reverses route segments until no crossing edges remain.",
        static (_, _) => new TwoOptSolver());

    public static SolverDescriptor SimulatedAnnealing { get; } = new(
        "simulated-annealing",
        SimulatedAnnealingSolver.DisplayName,
        "O(steps × iterations)",
        "Randomised 2-opt moves that occasionally accept worse routes to escape local optima.",
        static (cityCount, seed) => new SimulatedAnnealingSolver(SimulatedAnnealingOptions.ForCityCount(cityCount, seed)));

    public static SolverDescriptor GeneticAlgorithm { get; } = new(
        "genetic",
        GeneticAlgorithmSolver.DisplayName,
        "O(generations × population × n)",
        "Evolves a population of routes with tournament selection, order crossover and inversion mutation.",
        static (cityCount, seed) => new GeneticAlgorithmSolver(GeneticAlgorithmOptions.ForCityCount(cityCount, seed)));

    public static IReadOnlyList<SolverDescriptor> All { get; } = [NearestNeighbor, TwoOpt, SimulatedAnnealing, GeneticAlgorithm];

    public static IReadOnlyList<string> Keys { get; } = [.. All.Select(static descriptor => descriptor.Key)];

    public static bool TryFind(string key, [NotNullWhen(true)] out SolverDescriptor? descriptor)
    {
        descriptor = All.FirstOrDefault(candidate => string.Equals(candidate.Key, key, StringComparison.OrdinalIgnoreCase));
        return descriptor is not null;
    }

    public static SolverDescriptor Find(string key) =>
        TryFind(key, out var descriptor)
            ? descriptor
            : throw new ArgumentException($"Unknown algorithm '{key}'. Expected one of: {string.Join(", ", Keys)}.", nameof(key));
}
