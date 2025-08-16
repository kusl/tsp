using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
        private readonly ILogger<Tour> _logger;
        private double? _cachedDistance;

        public IReadOnlyList<City> Cities => _cities.AsReadOnly();
        public double TotalDistance => _cachedDistance ??= CalculateTotalDistance();

        public Tour(IEnumerable<City> cities, double[,] distanceMatrix, ILogger<Tour>? logger = null)
        {
            _cities = cities.ToList();
            _distanceMatrix = distanceMatrix;
            _logger = logger ?? NullLogger<Tour>.Instance;
        }

        private double CalculateTotalDistance()
        {
            if (_cities.Count < 2)
            {
                _logger.LogDebug("Tour has less than 2 cities, returning distance 0");
                return 0;
            }

            var distance = 0.0;
            for (int i = 0; i < _cities.Count - 1; i++)
            {
                distance += _distanceMatrix[_cities[i].Id, _cities[i + 1].Id];
            }
            // Return to start
            distance += _distanceMatrix[_cities[^1].Id, _cities[0].Id];
            
            _logger.LogTrace("Calculated total distance: {Distance:F2} for {CityCount} cities", distance, _cities.Count);
            return distance;
        }

        public Tour Clone(ILogger<Tour>? logger = null) => new Tour(_cities, _distanceMatrix, logger ?? _logger);

        public void SwapCities(int index1, int index2)
        {
            if (index1 == index2) return;
            
            var city1 = _cities[index1];
            var city2 = _cities[index2];
            
            (_cities[index1], _cities[index2]) = (_cities[index2], _cities[index1]);
            _cachedDistance = null;
            
            _logger.LogTrace("Swapped cities {City1} and {City2}", city1.Name, city2.Name);
        }

        public void Reverse(int start, int end)
        {
            _logger.LogTrace("Reversing tour segment from index {Start} to {End}", start, end);
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
        protected readonly ILogger _logger;

        protected TspSolverBase(ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        public abstract string Name { get; }
        public event EventHandler<TspProgressEventArgs>? ProgressChanged;

        protected double[,] BuildDistanceMatrix(IReadOnlyList<City> cities)
        {
            var n = cities.Count;
            var matrix = new double[n, n];

            _logger.LogDebug("Building distance matrix for {CityCount} cities", n);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = cities[i].DistanceTo(cities[j]);
                }
            }

            _logger.LogDebug("Distance matrix built successfully");
            return matrix;
        }

        protected void OnProgressChanged(int iteration, double currentBest, string message = "")
        {
            _logger.LogTrace("Algorithm progress: Iteration {Iteration}, Best Distance {Distance:F2}, Message: {Message}", 
                iteration, currentBest, message);
                
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

        public NearestNeighborSolver(ILogger<NearestNeighborSolver>? logger = null) : base(logger)
        {
        }

        public override Task<Tour> SolveAsync(IReadOnlyList<City> cities, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                _logger.LogInformation("Starting Nearest Neighbor algorithm for {CityCount} cities", cities.Count);

                if (cities.Count < 2)
                {
                    _logger.LogWarning("Less than 2 cities provided, returning minimal tour");
                    return new Tour(cities, BuildDistanceMatrix(cities));
                }

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

                        _logger.LogTrace("Added city {CityName} (distance: {Distance:F2})", cities[nearest].Name, nearestDistance);
                    }

                    OnProgressChanged(i, new Tour(route, distanceMatrix).TotalDistance, $"Added city {cities[current].Name}");
                }

                var finalTour = new Tour(route, distanceMatrix);
                _logger.LogInformation("Nearest Neighbor completed: Distance {Distance:F2}", finalTour.TotalDistance);
                
                return finalTour;
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

        public TwoOptSolver(int maxIterations = 1000, ILogger<TwoOptSolver>? logger = null) : base(logger)
        {
            _maxIterations = maxIterations;
        }

        public override async Task<Tour> SolveAsync(IReadOnlyList<City> cities, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting 2-Opt algorithm for {CityCount} cities (max iterations: {MaxIterations})", 
                cities.Count, _maxIterations);

            // Start with nearest neighbor solution
            var nnSolver = new NearestNeighborSolver(_logger as ILogger<NearestNeighborSolver>);
            var tour = await nnSolver.SolveAsync(cities, cancellationToken);

            _logger.LogDebug("Initial tour from Nearest Neighbor: {Distance:F2}", tour.TotalDistance);

            return await Task.Run(() => Improve2Opt(tour, cancellationToken), cancellationToken);
        }

        private Tour Improve2Opt(Tour tour, CancellationToken cancellationToken)
        {
            var improved = true;
            var iteration = 0;
            var bestTour = tour.Clone();
            var initialDistance = bestTour.TotalDistance;

            _logger.LogDebug("Starting 2-Opt improvement from distance {InitialDistance:F2}", initialDistance);

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
                            _logger.LogTrace("2-Opt improvement found: {OldDistance:F2} -> {NewDistance:F2}", 
                                bestTour.TotalDistance, newTour.TotalDistance);
                            bestTour = newTour;
                            improved = true;
                        }
                    }
                }

                iteration++;
                OnProgressChanged(iteration, bestTour.TotalDistance, $"2-Opt iteration {iteration}");
            }

            var finalImprovement = ((initialDistance - bestTour.TotalDistance) / initialDistance) * 100;
            _logger.LogInformation("2-Opt completed after {Iterations} iterations. " +
                                 "Distance: {FinalDistance:F2} (improved by {Improvement:F1}%)", 
                                 iteration, bestTour.TotalDistance, finalImprovement);

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
            double initialTemperature = 10000,
            double coolingRate = 0.9995,
            int iterationsPerTemperature = 1000,
            int? seed = null,
            ILogger<SimulatedAnnealingSolver>? logger = null) : base(logger)
        {
            _initialTemperature = initialTemperature;
            _coolingRate = coolingRate;
            _iterationsPerTemperature = iterationsPerTemperature;
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public override async Task<Tour> SolveAsync(IReadOnlyList<City> cities, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting Simulated Annealing for {CityCount} cities " +
                                 "(temp: {InitialTemp}, cooling: {CoolingRate}, iterations per temp: {IterationsPerTemp})", 
                                 cities.Count, _initialTemperature, _coolingRate, _iterationsPerTemperature);

            // Start with nearest neighbor solution
            var nnSolver = new NearestNeighborSolver(_logger as ILogger<NearestNeighborSolver>);
            var initialTour = await nnSolver.SolveAsync(cities, cancellationToken);

            return await Task.Run(() => RunSimulatedAnnealing(initialTour, cancellationToken), cancellationToken);
        }

        private Tour RunSimulatedAnnealing(Tour initialTour, CancellationToken cancellationToken)
        {
            var currentTour = initialTour.Clone();
            var bestTour = currentTour.Clone();
            var temperature = _initialTemperature;
            var iteration = 0;
            var acceptedMoves = 0;
            var rejectedMoves = 0;
            var initialDistance = initialTour.TotalDistance;

            _logger.LogDebug("Starting SA from initial distance: {InitialDistance:F2}", initialDistance);

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
                            acceptedMoves++;

                            if (currentTour.TotalDistance < bestTour.TotalDistance)
                            {
                                bestTour = currentTour.Clone();
                                _logger.LogTrace("New best solution found: {Distance:F2} at temperature {Temperature:F2}", 
                                    bestTour.TotalDistance, temperature);
                            }
                        }
                        else
                        {
                            rejectedMoves++;
                        }
                    }

                    iteration++;
                    if (iteration % 1000 == 0)
                    {
                        OnProgressChanged(iteration, bestTour.TotalDistance,
                            $"Temperature: {temperature:F2}, Best: {bestTour.TotalDistance:F2}");
                    }
                }

                temperature *= _coolingRate;
            }

            var finalImprovement = ((initialDistance - bestTour.TotalDistance) / initialDistance) * 100;
            var acceptanceRate = (double)acceptedMoves / (acceptedMoves + rejectedMoves) * 100;

            _logger.LogInformation("Simulated Annealing completed after {Iterations} iterations. " +
                                 "Distance: {FinalDistance:F2} (improved by {Improvement:F1}%). " +
                                 "Acceptance rate: {AcceptanceRate:F1}%", 
                                 iteration, bestTour.TotalDistance, finalImprovement, acceptanceRate);

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
            int? seed = null,
            ILogger<GeneticAlgorithmSolver>? logger = null) : base(logger)
        {
            _populationSize = populationSize;
            _generations = generations;
            _mutationRate = mutationRate;
            _elitismRate = elitismRate;
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public static GeneticAlgorithmSolver CreateScaledGeneticSolver(int cityCount, int? seed = null, ILogger<GeneticAlgorithmSolver>? logger = null)
        {
            return new GeneticAlgorithmSolver(
                populationSize: Math.Max(200, cityCount * 2),
                generations: Math.Max(1000, cityCount * 10),
                mutationRate: 0.1,
                elitismRate: 0.1,
                seed: seed,
                logger: logger
            );
        }

        public override Task<Tour> SolveAsync(IReadOnlyList<City> cities, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting Genetic Algorithm for {CityCount} cities " +
                                 "(population: {Population}, generations: {Generations}, " +
                                 "mutation rate: {MutationRate:F3}, elitism rate: {ElitismRate:F3})", 
                                 cities.Count, _populationSize, _generations, _mutationRate, _elitismRate);

            return Task.Run(() => RunGeneticAlgorithm(cities, cancellationToken), cancellationToken);
        }

        private Tour RunGeneticAlgorithm(IReadOnlyList<City> cities, CancellationToken cancellationToken)
        {
            var distanceMatrix = BuildDistanceMatrix(cities);
            var population = InitializePopulation(cities, distanceMatrix);
            var bestTour = population.OrderBy(t => t.TotalDistance).First();
            var initialBest = bestTour.TotalDistance;
            var generationsWithoutImprovement = 0;

            _logger.LogDebug("Initial population created. Best distance: {BestDistance:F2}", initialBest);

            for (int generation = 0; generation < _generations; generation++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                population = EvolvePopulation(population, distanceMatrix);

                var generationBest = population.OrderBy(t => t.TotalDistance).First();
                if (generationBest.TotalDistance < bestTour.TotalDistance)
                {
                    _logger.LogDebug("Generation {Generation}: New best solution {Distance:F2}", 
                        generation, generationBest.TotalDistance);
                    bestTour = generationBest.Clone();
                    generationsWithoutImprovement = 0;
                }
                else
                {
                    generationsWithoutImprovement++;
                }

                if (generation % 50 == 0)
                {
                    OnProgressChanged(generation, bestTour.TotalDistance,
                        $"Generation {generation}, Best: {bestTour.TotalDistance:F2}");
                }

                // Early stopping if no improvement for many generations
                if (generationsWithoutImprovement > _generations / 4)
                {
                    _logger.LogDebug("Early stopping at generation {Generation} due to no improvement", generation);
                    break;
                }
            }

            var finalImprovement = ((initialBest - bestTour.TotalDistance) / initialBest) * 100;
            _logger.LogInformation("Genetic Algorithm completed. " +
                                 "Distance: {FinalDistance:F2} (improved by {Improvement:F1}%)", 
                                 bestTour.TotalDistance, finalImprovement);

            return bestTour;
        }

        private List<Tour> InitializePopulation(IReadOnlyList<City> cities, double[,] distanceMatrix)
        {
            var population = new List<Tour>();

            _logger.LogDebug("Initializing population of {PopulationSize} individuals", _populationSize);

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
    /// Factory for creating TSP solvers with logging
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

        public static ITspSolver CreateSolver(SolverType type, ILoggerFactory? loggerFactory = null)
        {
            return type switch
            {
                SolverType.NearestNeighbor => new NearestNeighborSolver(loggerFactory?.CreateLogger<NearestNeighborSolver>()),
                SolverType.TwoOpt => new TwoOptSolver(logger: loggerFactory?.CreateLogger<TwoOptSolver>()),
                SolverType.SimulatedAnnealing => new SimulatedAnnealingSolver(logger: loggerFactory?.CreateLogger<SimulatedAnnealingSolver>()),
                SolverType.GeneticAlgorithm => new GeneticAlgorithmSolver(logger: loggerFactory?.CreateLogger<GeneticAlgorithmSolver>()),
                _ => throw new ArgumentException($"Unknown solver type: {type}")
            };
        }

        public static IEnumerable<ITspSolver> CreateAllSolvers(ILoggerFactory? loggerFactory = null)
        {
            yield return new NearestNeighborSolver(loggerFactory?.CreateLogger<NearestNeighborSolver>());
            yield return new TwoOptSolver(logger: loggerFactory?.CreateLogger<TwoOptSolver>());
            yield return new SimulatedAnnealingSolver(logger: loggerFactory?.CreateLogger<SimulatedAnnealingSolver>());
            yield return new GeneticAlgorithmSolver(logger: loggerFactory?.CreateLogger<GeneticAlgorithmSolver>());
        }
    }

    /// <summary>
    /// Service for generating test data
    /// </summary>
    public sealed class TspDataGenerator
    {
        private readonly Random _random;
        private readonly ILogger<TspDataGenerator> _logger;

        public TspDataGenerator(int? seed = null, ILogger<TspDataGenerator>? logger = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
            _logger = logger ?? NullLogger<TspDataGenerator>.Instance;
        }

        public IReadOnlyList<City> GenerateRandomCities(int count, double maxX = 100, double maxY = 100)
        {
            _logger.LogDebug("Generating {Count} random cities in area {MaxX}x{MaxY}", count, maxX, maxY);
            
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

            _logger.LogInformation("Generated {Count} random cities", count);
            return cities;
        }

        public IReadOnlyList<City> GenerateCircularCities(int count, double radius = 50, double centerX = 50, double centerY = 50)
        {
            _logger.LogDebug("Generating {Count} cities in circular pattern (radius: {Radius})", count, radius);
            
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

            _logger.LogInformation("Generated {Count} cities in circular pattern", count);
            return cities;
        }

        public IReadOnlyList<City> GenerateGridCities(int rows, int cols, double spacing = 10)
        {
            _logger.LogDebug("Generating {Rows}x{Cols} cities in grid pattern (spacing: {Spacing})", rows, cols, spacing);
            
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

            _logger.LogInformation("Generated {Count} cities in {Rows}x{Cols} grid pattern", cities.Count, rows, cols);
            return cities;
        }
    }

    /// <summary>
    /// Service for comparing different TSP solvers
    /// </summary>
    public sealed class TspBenchmark
    {
        private readonly ILogger<TspBenchmark> _logger;

        public TspBenchmark(ILogger<TspBenchmark>? logger = null)
        {
            _logger = logger ?? NullLogger<TspBenchmark>.Instance;
        }

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
            var solverList = solvers.ToList();
            _logger.LogInformation("Starting benchmark with {CityCount} cities and {SolverCount} algorithms", 
                cities.Count, solverList.Count);

            var results = new List<BenchmarkResult>();

            foreach (var solver in solverList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogDebug("Running benchmark for {SolverName}", solver.Name);
                var startTime = DateTime.UtcNow;
                var tour = await solver.SolveAsync(cities, cancellationToken);
                var executionTime = DateTime.UtcNow - startTime;

                var result = new BenchmarkResult(solver.Name, tour.TotalDistance, executionTime, tour);
                results.Add(result);

                _logger.LogInformation("Benchmark completed for {SolverName}: Distance {Distance:F2}, Time {TimeMs}ms", 
                    solver.Name, tour.TotalDistance, executionTime.TotalMilliseconds);
            }

            var sortedResults = results.OrderBy(r => r.Distance).ToList();
            _logger.LogInformation("Benchmark completed. Winner: {Winner} with distance {Distance:F2}", 
                sortedResults.First().SolverName, sortedResults.First().Distance);

            return sortedResults;
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