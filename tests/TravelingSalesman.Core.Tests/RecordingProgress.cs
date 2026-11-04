using TravelingSalesman.Core.Solvers;

namespace TravelingSalesman.Core.Tests;

internal sealed class RecordingProgress : IProgress<SolverProgress>
{
    public List<SolverProgress> Reports { get; } = [];

    public void Report(SolverProgress value) => Reports.Add(value);
}
