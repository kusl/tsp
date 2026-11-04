namespace TravelingSalesman.Core.Solvers;

public sealed class GeneticAlgorithmSolver(GeneticAlgorithmOptions? options = null) : ITspSolver
{
    public const string DisplayName = "Genetic Algorithm";

    private static readonly Comparison<Individual> ByLength = static (left, right) => left.Length.CompareTo(right.Length);

    private readonly GeneticAlgorithmOptions _options = options ?? new GeneticAlgorithmOptions();

    public string Name => DisplayName;

    public Tour Solve(TspProblem problem, IProgress<SolverProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var distances = problem.Distances;
        var greedy = NearestNeighborSolver.BuildOrder(distances, cancellationToken);
        var count = greedy.Length;

        if (count < 4)
        {
            var trivial = new Tour(problem, greedy);
            progress?.Report(new SolverProgress(0, trivial.Distance));
            return trivial;
        }

        var random = _options.Seed is { } seed ? new Random(seed) : new Random();
        var size = _options.PopulationSize;
        var eliteCount = Math.Clamp((int)Math.Round(size * _options.EliteFraction), 1, size - 1);

        var population = new Individual[size];
        population[0] = Individual.Create(greedy, distances);
        for (var i = 1; i < size; i++)
        {
            population[i] = Individual.Create(RandomOrder(count, random), distances);
        }

        Array.Sort(population, ByLength);
        var best = population[0];
        var stagnantGenerations = 0;

        for (var generation = 1;
             generation <= _options.Generations && stagnantGenerations < _options.StagnationLimit;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var next = new Individual[size];
            Array.Copy(population, next, eliteCount);

            for (var i = eliteCount; i < size; i++)
            {
                var child = OrderCrossover(Select(population, random).Genes, Select(population, random).Genes, random);
                if (random.NextDouble() < _options.MutationRate)
                {
                    Invert(child, random);
                }

                next[i] = Individual.Create(child, distances);
            }

            Array.Sort(next, ByLength);
            population = next;

            if (population[0].Length < best.Length - TourMath.Epsilon)
            {
                best = population[0];
                stagnantGenerations = 0;
            }
            else
            {
                stagnantGenerations++;
            }

            progress?.Report(new SolverProgress(generation, best.Length));
        }

        return new Tour(problem, best.Genes);
    }

    private Individual Select(Individual[] population, Random random)
    {
        var winner = population[random.Next(population.Length)];
        for (var round = 1; round < _options.TournamentSize; round++)
        {
            var challenger = population[random.Next(population.Length)];
            if (challenger.Length < winner.Length)
            {
                winner = challenger;
            }
        }

        return winner;
    }

    private static int[] RandomOrder(int count, Random random)
    {
        var order = new int[count];
        for (var i = 0; i < count; i++)
        {
            order[i] = i;
        }

        random.Shuffle(order.AsSpan(1));
        return order;
    }

    private static int[] OrderCrossover(int[] first, int[] second, Random random)
    {
        var count = first.Length;
        var child = new int[count];
        var used = new bool[count];
        used[0] = true;

        var start = random.Next(1, count);
        var end = random.Next(start, count);
        for (var i = start; i <= end; i++)
        {
            child[i] = first[i];
            used[first[i]] = true;
        }

        var source = 1;
        for (var i = 1; i < count; i++)
        {
            if (i >= start && i <= end)
            {
                continue;
            }

            while (used[second[source]])
            {
                source++;
            }

            child[i] = second[source];
            used[second[source]] = true;
        }

        return child;
    }

    private static void Invert(int[] order, Random random)
    {
        var start = random.Next(1, order.Length - 1);
        var end = random.Next(start + 1, order.Length);
        TourMath.Reverse(order, start, end);
    }

    private sealed record Individual(int[] Genes, double Length)
    {
        public static Individual Create(int[] genes, DistanceMatrix distances) => new(genes, TourMath.Length(genes, distances));
    }
}
