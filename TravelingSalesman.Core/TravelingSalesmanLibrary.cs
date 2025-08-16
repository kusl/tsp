using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TravelingSalesman.Core
{
    /// <summary>
    /// Represents a city in the TSP problem
    /// </summary>
    public sealed record City(int Id, string Name, double X, double Y)
    {
        public double DistanceTo(City other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    /// <summary>
    /// Represents a tour through cities
    /// </summary>
    public sealed class Tour
    {
        private readonly List<City> _cities;
        private readonly double[,] _distanceMatrix;
        private double? _cachedDistance;

        public IReadOnlyList<City> Cities => _cities.AsReadOnly();
        public double TotalDistance => _cachedDistance ??= CalculateTotalDistance();

        public Tour(IEnumerable<City> cities, double[,] distanceMatrix)
        {
            _cities = cities.ToList();
            _distanceMatrix = distanceMatrix;
        }

        private double CalculateTotalDistance()
        {
            if (_cities.Count < 2) return 0;

            var distance = 0.0;
            for (int i = 0; i < _cities.Count - 1; i++)
            {
                distance += _distanceMatrix[_cities[i].Id, _cities[i + 1].Id];
            }
            // Return to start
            distance += _distanceMatrix[_cities[^1].Id, _cities[0].Id];
            return distance;
        }

        public Tour Clone() => new Tour(_cities, _distanceMatrix);

        public void SwapCities(int index1, int index2)
        {
            (_cities[index1], _cities[index2]) = (_cities[index2], _cities[index1]);
            _cachedDistance = null;
        }

        public void Reverse(int start, int end)
        {
            while (start < end)
            {
                SwapCities(start, end);
                start++;
                end--;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Tour Distance: {TotalDistance:F2}");
            sb.AppendLine("Route:");
            foreach (var city in _cities)
            {
                sb.Append($"{city.Name} -> ");
            }
            sb.Append(_cities[0].Name); // Return to start
            return sb.ToString();
        }
    }

    /// <summary>
    /// Interface for TSP solving algorithms
    /// </summary>
    public interface ITspSolver
    {
        string Name { get; }
        Task<Tour> SolveAsync(IReadOnlyList<City> cities, CancellationToken cancellationToken = default);
        event EventHandler<TspProgressEventArgs>? ProgressChanged;
    }

    /// <summary>
    /// Event args for progress reporting
    /// </summary>
    public sealed class TspProgressEventArgs : EventArgs
    {
        public int Iteration { get; init; }
        public double CurrentBestDistance { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    /// <summary>
    /// Base class for TSP solvers
    /// </summary>
    public abstract class TspSolverBase : ITspSolver
    {
        public abstract string Name { get; }
        public event EventHandler<TspProgressEventArgs>? ProgressChanged;

        protected double[,] BuildDistanceMatrix(IReadOnlyList<City> cities)
        {
            var n = cities.Count;
            var matrix = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = cities[i].DistanceTo(cities[j]);
                }
            }

            return matrix;
        }

        protected void OnProgressChanged(int iteration, double currentBest, string message = "")
        {
            ProgressChanged?.Invoke(this, new TspProgressEventArgs
            {
                Iteration = iteration,
                CurrentBestDistance = currentBest,
                Message = message
            });
        }

        public abstract Task<Tour> SolveAsync(IReadOnlyList<City> cities, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Nearest Neighbor heuristic solver
    /// </summary>
    public sealed class NearestNeighborSolver : TspSolverBase
    {
        public override string Name => "Nearest Neighbor";

        public override Task<Tour> SolveAsync(IReadOnlyList<City> cities, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                if (cities.Count < 2)
                    return new Tour(cities, BuildDistanceMatrix(cities));

                var distanceMatrix = BuildDistanceMatrix(cities);
                var visited = new bool[cities.Count];
                var route = new List<City> { cities[0] };
                visited[0] = true;

                var current = 0;
                for (int i = 1; i < cities.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var nearest = -1;
                    var nearestDistance = double.MaxValue;

                    for (int j = 0; j < cities.Count; j++)
                    {
                        if (!visited[j] && distanceMatrix[current, j] < nearestDistance)
                        {
                            nearest = j;
                            nearestDistance = distanceMatrix[current, j];
                        }
                    }

                    if (nearest != -1)
                    {
                        visited[nearest] = true;
                        route.Add(cities[nearest]);
                        current = nearest;
                    }

                    OnProgressChanged(i, new Tour(route, distanceMatrix).TotalDistance, $"Added city {cities[current].Name}");
                }

                return new Tour(route, distanceMatrix);
            }, cancellationToken);
        }
    }

    /// <summary>
    /// 2-Opt local search improvement solver
    /// </summary>
    public sealed class TwoOptSolver : TspSolverBase
    {
        private readonly int _maxIterations;

        public override string Name => "2-Opt";

        public TwoOptSolver(int maxIterations = 1000)
        {
            _maxIterations = maxIterations;
        }

        public override async Task<Tour> SolveAsync(IReadOnlyList<City> cities, CancellationToken cancellationToken = default)
        {
            // Start with nearest neighbor solution
            var nnSolver = new NearestNeighborSolver();
            var tour = await nnSolver.SolveAsync(cities, cancellationToken);

            return await Task.Run(() => Improve2Opt(tour, cancellationToken), cancellationToken);
        }

        private Tour Improve2Opt(Tour tour, CancellationToken cancellationToken)
        {
            var improved = true;
            var iteration = 0;
            var bestTour = tour.Clone();

            while (improved && iteration < _maxIterations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                improved = false;

                for (int i = 1; i < tour.Cities.Count - 2; i++)
                {
                    for (int j = i + 1; j < tour.Cities.Count; j++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // Try reversing the tour between i and j
                        var newTour = bestTour.Clone();
                        newTour.Reverse(i, j);

                        if (newTour.TotalDistance < bestTour.TotalDistance)
                        {
                            bestTour = newTour;
                            improved = true;
                        }
                    }
                }

                iteration++;
                OnProgressChanged(iteration, bestTour.TotalDistance, $"2-Opt iteration {iteration}");
            }

            return bestTour;
        }
    }

    /// <summary>
    /// Simulated Annealing solver for TSP
    /// </summary>
    public sealed class SimulatedAnnealingSolver : TspSolverBase
    {
        private readonly double _initialTemperature;
        private readonly double _coolingRate;
        private readonly int _iterationsPerTemperature;
        private readonly Random _random;

        public override string Name => "Simulated Annealing";

        public SimulatedAnnealingSolver(
            double initialTemperature = 10000,  // Was 1000 - needs to be higher for more cities
            double coolingRate = 0.9995,        // Was 0.995 - slower cooling
            int iterationsPerTemperature = 1000, // Was 100 - more iterations
            int? seed = null)
        {
            _initialTemperature = initialTemperature;
            _coolingRate = coolingRate;
            _iterationsPerTemperature = iterationsPerTemperature;
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public override async Task<Tour> SolveAsync(IReadOnlyList<City> cities, CancellationToken cancellationToken = default)
        {
            // Start with nearest neighbor solution
            var nnSolver = new NearestNeighborSolver();
            var initialTour = await nnSolver.SolveAsync(cities, cancellationToken);

            return await Task.Run(() => RunSimulatedAnnealing(initialTour, cancellationToken), cancellationToken);
        }

        private Tour RunSimulatedAnnealing(Tour initialTour, CancellationToken cancellationToken)
        {
            var currentTour = initialTour.Clone();
            var bestTour = currentTour.Clone();
            var temperature = _initialTemperature;
            var iteration = 0;

            while (temperature > 0.1)
            {
                for (int i = 0; i < _iterationsPerTemperature; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var newTour = currentTour.Clone();

                    // Random perturbation: swap two random cities
                    var index1 = _random.Next(1, newTour.Cities.Count);
                    var index2 = _random.Next(1, newTour.Cities.Count);

                    if (index1 != index2)
                    {
                        newTour.SwapCities(index1, index2);

                        var deltaDistance = newTour.TotalDistance - currentTour.TotalDistance;

                        // Accept or reject the new solution
                        if (deltaDistance < 0 || _random.NextDouble() < Math.Exp(-deltaDistance / temperature))
                        {
                            currentTour = newTour;

                            if (currentTour.TotalDistance < bestTour.TotalDistance)
                            {
                                bestTour = currentTour.Clone();
                            }
                        }
                    }

                    iteration++;
                    if (iteration % 100 == 0)
                    {
                        OnProgressChanged(iteration, bestTour.TotalDistance,
                            $"Temperature: {temperature:F2}, Best: {bestTour.TotalDistance:F2}");
                    }
                }

                temperature *= _coolingRate;
            }

            return bestTour;
        }
    }

    /// <summary>
    /// Genetic Algorithm solver for TSP
    /// </summary>
    public sealed class GeneticAlgorithmSolver : TspSolverBase
    {
        private readonly int _populationSize;
        private readonly int _generations;
        private readonly double _mutationRate;
        private readonly double _elitismRate;
        private readonly Random _random;

        public override string Name => "Genetic Algorithm";

        public GeneticAlgorithmSolver(
            int populationSize = 100,
            int generations = 500,
            double mutationRate = 0.02,
            double elitismRate = 0.2,
            int? seed = null)
        {
            _populationSize = populationSize;
            _generations = generations;
            _mutationRate = mutationRate;
            _elitismRate = elitismRate;
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        // Better Genetic Algorithm parameters that scale with problem size
        public static GeneticAlgorithmSolver CreateScaledGeneticSolver(int cityCount, int? seed = null)
        {
            return new GeneticAlgorithmSolver(
                populationSize: Math.Max(200, cityCount * 2),  // Scale with city count
                generations: Math.Max(1000, cityCount * 10),   // More generations for larger problems
                mutationRate: 0.1,                             // Higher mutation
                elitismRate: 0.1,                              // Less elitism
                seed: seed
            );
        }

        public override Task<Tour> SolveAsync(IReadOnlyList<City> cities, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => RunGeneticAlgorithm(cities, cancellationToken), cancellationToken);
        }

        private Tour RunGeneticAlgorithm(IReadOnlyList<City> cities, CancellationToken cancellationToken)
        {
            var distanceMatrix = BuildDistanceMatrix(cities);
            var population = InitializePopulation(cities, distanceMatrix);
            var bestTour = population.OrderBy(t => t.TotalDistance).First();

            for (int generation = 0; generation < _generations; generation++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                population = EvolvePopulation(population, distanceMatrix);

                var generationBest = population.OrderBy(t => t.TotalDistance).First();
                if (generationBest.TotalDistance < bestTour.TotalDistance)
                {
                    bestTour = generationBest.Clone();
                }

                if (generation % 10 == 0)
                {
                    OnProgressChanged(generation, bestTour.TotalDistance,
                        $"Generation {generation}, Best: {bestTour.TotalDistance:F2}");
                }
            }

            return bestTour;
        }

        private List<Tour> InitializePopulation(IReadOnlyList<City> cities, double[,] distanceMatrix)
        {
            var population = new List<Tour>();

            for (int i = 0; i < _populationSize; i++)
            {
                var shuffled = cities.Skip(1).OrderBy(_ => _random.Next()).ToList();
                shuffled.Insert(0, cities[0]); // Keep first city fixed
                population.Add(new Tour(shuffled, distanceMatrix));
            }

            return population;
        }

        private List<Tour> EvolvePopulation(List<Tour> population, double[,] distanceMatrix)
        {
            var newPopulation = new List<Tour>();

            // Keep elite individuals
            var eliteCount = (int)(_populationSize * _elitismRate);
            var elite = population.OrderBy(t => t.TotalDistance).Take(eliteCount).ToList();
            newPopulation.AddRange(elite.Select(t => t.Clone()));

            // Fill rest with offspring
            while (newPopulation.Count < _populationSize)
            {
                var parent1 = TournamentSelection(population);
                var parent2 = TournamentSelection(population);
                var child = Crossover(parent1, parent2, distanceMatrix);

                if (_random.NextDouble() < _mutationRate)
                {
                    Mutate(child);
                }

                newPopulation.Add(child);
            }

            return newPopulation;
        }

        private Tour TournamentSelection(List<Tour> population, int tournamentSize = 5)
        {
            var tournament = new List<Tour>();

            for (int i = 0; i < tournamentSize; i++)
            {
                tournament.Add(population[_random.Next(population.Count)]);
            }

            return tournament.OrderBy(t => t.TotalDistance).First();
        }

        private Tour Crossover(Tour parent1, Tour parent2, double[,] distanceMatrix)
        {
            var cities = parent1.Cities.ToList();
            var start = _random.Next(1, cities.Count - 1);
            var end = _random.Next(start + 1, cities.Count);

            var childCities = new List<City> { cities[0] }; // Keep first city fixed
            var segment = parent1.Cities.Skip(start).Take(end - start).ToList();

            foreach (var city in segment)
            {
                if (city.Id != 0) // Skip first city
                    childCities.Add(city);
            }

            foreach (var city in parent2.Cities)
            {
                if (!childCities.Contains(city) && city.Id != 0)
                {
                    childCities.Add(city);
                }
            }

            return new Tour(childCities, distanceMatrix);
        }

        private void Mutate(Tour tour)
        {
            var index1 = _random.Next(1, tour.Cities.Count);
            var index2 = _random.Next(1, tour.Cities.Count);

            if (index1 != index2)
            {
                tour.SwapCities(index1, index2);
            }
        }
    }

    /// <summary>
    /// Factory for creating TSP solvers
    /// </summary>
    public static class TspSolverFactory
    {
        public enum SolverType
        {
            NearestNeighbor,
            TwoOpt,
            SimulatedAnnealing,
            GeneticAlgorithm
        }

        public static ITspSolver CreateSolver(SolverType type)
        {
            return type switch
            {
                SolverType.NearestNeighbor => new NearestNeighborSolver(),
                SolverType.TwoOpt => new TwoOptSolver(),
                SolverType.SimulatedAnnealing => new SimulatedAnnealingSolver(),
                SolverType.GeneticAlgorithm => new GeneticAlgorithmSolver(),
                _ => throw new ArgumentException($"Unknown solver type: {type}")
            };
        }

        public static IEnumerable<ITspSolver> CreateAllSolvers()
        {
            yield return new NearestNeighborSolver();
            yield return new TwoOptSolver();
            yield return new SimulatedAnnealingSolver();
            yield return new GeneticAlgorithmSolver();
        }
    }

    /// <summary>
    /// Service for generating test data
    /// </summary>
    public sealed class TspDataGenerator
    {
        private readonly Random _random;

        public TspDataGenerator(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public IReadOnlyList<City> GenerateRandomCities(int count, double maxX = 100, double maxY = 100)
        {
            var cities = new List<City>();

            for (int i = 0; i < count; i++)
            {
                cities.Add(new City(
                    i,
                    $"City_{i}",
                    _random.NextDouble() * maxX,
                    _random.NextDouble() * maxY
                ));
            }

            return cities;
        }

        public IReadOnlyList<City> GenerateCircularCities(int count, double radius = 50, double centerX = 50, double centerY = 50)
        {
            var cities = new List<City>();
            var angleStep = 2 * Math.PI / count;

            for (int i = 0; i < count; i++)
            {
                var angle = i * angleStep;
                cities.Add(new City(
                    i,
                    $"City_{i}",
                    centerX + radius * Math.Cos(angle),
                    centerY + radius * Math.Sin(angle)
                ));
            }

            return cities;
        }

        public IReadOnlyList<City> GenerateGridCities(int rows, int cols, double spacing = 10)
        {
            var cities = new List<City>();
            var id = 0;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    cities.Add(new City(
                        id,
                        $"City_{id}",
                        col * spacing,
                        row * spacing
                    ));
                    id++;
                }
            }

            return cities;
        }
    }

    /// <summary>
    /// Service for comparing different TSP solvers
    /// </summary>
    public sealed class TspBenchmark
    {
        public sealed record BenchmarkResult(
            string SolverName,
            double Distance,
            TimeSpan ExecutionTime,
            Tour Tour
        );

        public async Task<IReadOnlyList<BenchmarkResult>> RunBenchmarkAsync(
            IReadOnlyList<City> cities,
            IEnumerable<ITspSolver> solvers,
            CancellationToken cancellationToken = default)
        {
            var results = new List<BenchmarkResult>();

            foreach (var solver in solvers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var startTime = DateTime.UtcNow;
                var tour = await solver.SolveAsync(cities, cancellationToken);
                var executionTime = DateTime.UtcNow - startTime;

                results.Add(new BenchmarkResult(
                    solver.Name,
                    tour.TotalDistance,
                    executionTime,
                    tour
                ));
            }

            return results.OrderBy(r => r.Distance).ToList();
        }

        public string FormatResults(IReadOnlyList<BenchmarkResult> results)
        {
            var sb = new StringBuilder();
            sb.AppendLine("\n=== TSP Solver Benchmark Results ===");
            sb.AppendLine($"{"Rank",-5} {"Solver",-20} {"Distance",-15} {"Time (ms)",-10} {"% from Best",-12}");
            sb.AppendLine(new string('-', 75));

            var bestDistance = results.First().Distance;

            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                var percentFromBest = ((result.Distance - bestDistance) / bestDistance) * 100;

                sb.AppendLine($"{i + 1,-5} {result.SolverName,-20} {result.Distance,-15:F2} " +
                            $"{result.ExecutionTime.TotalMilliseconds,-10:F1} {percentFromBest,-12:F2}%");
            }

            return sb.ToString();
        }
    }
}