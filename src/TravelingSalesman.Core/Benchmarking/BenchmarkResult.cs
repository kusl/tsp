namespace TravelingSalesman.Core.Benchmarking;

public sealed record BenchmarkResult(string SolverName, Tour Tour, TimeSpan Elapsed)
{
    public double Distance => Tour.Distance;
}
