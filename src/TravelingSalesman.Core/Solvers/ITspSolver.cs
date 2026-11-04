namespace TravelingSalesman.Core.Solvers;

public interface ITspSolver
{
    string Name { get; }

    Tour Solve(TspProblem problem, IProgress<SolverProgress>? progress = null, CancellationToken cancellationToken = default);
}
