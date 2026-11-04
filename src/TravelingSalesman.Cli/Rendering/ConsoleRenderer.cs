using System.Globalization;
using TravelingSalesman.Core;
using TravelingSalesman.Core.Benchmarking;
using TravelingSalesman.Core.Solvers;

namespace TravelingSalesman.Cli.Rendering;

internal sealed class ConsoleRenderer(TextWriter output)
{
    public const int RoutePreviewStops = 10;

    private readonly TextWriter _output = output ?? throw new ArgumentNullException(nameof(output));

    public void Solution(Tour tour, string algorithm, TimeSpan elapsed, bool showFullRoute, bool showPlot)
    {
        ArgumentNullException.ThrowIfNull(tour);

        Line($"Algorithm : {algorithm}");
        Line($"Cities    : {tour.Cities.Count}");
        Line($"Distance  : {tour.Distance:F2}");
        Line($"Elapsed   : {elapsed.TotalMilliseconds:F1} ms");
        Line($"Route     : {tour.FormatRoute(showFullRoute ? null : RoutePreviewStops)}");

        if (showPlot)
        {
            _output.WriteLine();
            foreach (var row in AsciiPlot.Render(tour))
            {
                _output.WriteLine(row);
            }
        }
    }

    public void Benchmark(IReadOnlyList<BenchmarkResult> results, int cityCount)
    {
        ArgumentNullException.ThrowIfNull(results);

        Line($"Benchmark of {results.Count} algorithms on {cityCount} cities");
        _output.WriteLine();
        Line($"{"Rank",-5} {"Algorithm",-22} {"Distance",12} {"vs best",9} {"Time (ms)",11}");
        _output.WriteLine(new string('-', 63));

        if (results.Count == 0)
        {
            return;
        }

        var best = results[0].Distance;
        for (var i = 0; i < results.Count; i++)
        {
            var result = results[i];
            var gap = best > 0 ? (result.Distance - best) / best : 0;
            Line($"{i + 1,-5} {result.SolverName,-22} {result.Distance,12:F2} {gap,9:P2} {result.Elapsed.TotalMilliseconds,11:F1}");
        }

        _output.WriteLine();
        Line($"Best: {results[0].SolverName} ({best:F2})");
    }

    public void Algorithms(IReadOnlyList<SolverDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);

        foreach (var descriptor in descriptors)
        {
            Line($"{descriptor.Key,-20} {descriptor.DisplayName}");
            Line($"{string.Empty,-20} Complexity: {descriptor.Complexity}");
            Line($"{string.Empty,-20} {descriptor.Summary}");
            _output.WriteLine();
        }
    }

    public void Cancelled() => _output.WriteLine("Cancelled.");

    private void Line(FormattableString text) => _output.WriteLine(text.ToString(CultureInfo.InvariantCulture));
}
