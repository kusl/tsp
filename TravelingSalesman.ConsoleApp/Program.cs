using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TravelingSalesman.Core;
using Serilog;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace TravelingSalesman.ConsoleApp
{
    class Program
    {
        private static ILoggerFactory _loggerFactory = null!;
        private static ILogger<Program> _logger = null!;

        static async Task Main(string[] args)
        {
            // Configure Serilog to write to both console and file
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("TravelingSalesman.Core", Serilog.Events.LogEventLevel.Debug)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/tsp-solver-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}")
                .Enrich.WithProperty("Application", "TSP-Solver")
                .Enrich.WithProperty("Version", GetAssemblyVersion())
                .CreateLogger();

            // Create Microsoft.Extensions.Logging factory with Serilog
            _loggerFactory = new SerilogLoggerFactory(Log.Logger);
            _logger = _loggerFactory.CreateLogger<Program>();

            _logger.LogInformation("TSP Solver v{Version} starting up", GetAssemblyVersion());

            try
            {
                PrintHeader();

                while (true)
                {
                    var option = ShowMainMenu();
                    _logger.LogDebug("User selected menu option: {Option}", option);

                    switch (option)
                    {
                        case 1:
                            await RunInteractiveSolver();
                            break;
                        case 2:
                            await RunBenchmark();
                            break;
                        case 3:
                            await RunDemonstration();
                            break;
                        case 4:
                            ShowAlgorithmInfo();
                            break;
                        case 5:
                            _logger.LogInformation("User requested application exit");
                            Log.Information("Thank you for using TSP Solver! Goodbye!");
                            return;
                        default:
                            _logger.LogWarning("Invalid menu option selected: {Option}", option);
                            Log.Error("Invalid option. Please try again.");
                            break;
                    }

                    if (option != 5)
                    {
                        Log.Information("Press any key to return to main menu...");
                        Console.ReadKey();
                        Console.WriteLine(); // Still need Console.WriteLine for newline after keypress
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogFatal(ex, "Unexpected error occurred in main application loop");
                Log.Fatal("An unexpected error occurred: {ErrorMessage}", ex.Message);
                Log.Information("Press any key to exit...");
                Console.ReadKey();
            }
            finally
            {
                _logger.LogInformation("TSP Solver shutting down");
                await Log.CloseAndFlushAsync();
            }
        }

        static async Task RunInteractiveSolver()
        {
            _logger.LogInformation("Starting interactive solver session");
            LogSectionHeader("Interactive TSP Solver");

            // Get number of cities
            Log.Information("How many cities? (minimum 2): ");
            if (!int.TryParse(Console.ReadLine(), out int cityCount) || cityCount < 2)
            {
                _logger.LogWarning("Invalid city count input, using default of 10");
                Log.Error("Invalid input. Using default of 10 cities.");
                cityCount = 10;
            }

            _logger.LogInformation("Interactive solver configured for {CityCount} cities", cityCount);

            // Warn for large numbers
            if (cityCount > 100)
            {
                _logger.LogWarning("Large city count requested: {CityCount}", cityCount);
                Log.Warning("⚠️ Note: {CityCount} cities may take significant time with some algorithms.", cityCount);
                Log.Warning("   Nearest Neighbor will be fast, but Genetic Algorithm may take minutes.");
                Log.Information("   Continue? (y/n): ");

                if (Console.ReadLine()?.ToLower() != "y")
                {
                    Log.Information("Using 50 cities instead.");
                    cityCount = 50;
                    _logger.LogInformation("User reduced city count to {CityCount}", cityCount);
                }
            }

            // Select data pattern
            Log.Information("Select city distribution pattern:");
            Log.Information("  1. Random");
            Log.Information("  2. Circular"); 
            Log.Information("  3. Grid");
            Log.Information("➤ Select pattern (1-3): ");

            var generator = new TspDataGenerator(42, _loggerFactory.CreateLogger<TspDataGenerator>());
            IReadOnlyList<City> cities;
            string pattern;

            var patternChoice = Console.ReadLine();
            switch (patternChoice)
            {
                case "2":
                    cities = generator.GenerateCircularCities(cityCount);
                    pattern = "circular";
                    Log.Information("✓ Generated {CityCount} cities in circular pattern", cityCount);
                    break;
                case "3":
                    var gridSize = (int)Math.Sqrt(cityCount);
                    cities = generator.GenerateGridCities(gridSize, gridSize + (cityCount - gridSize * gridSize) / gridSize + 1);
                    cities = cities.Take(cityCount).ToList();
                    pattern = "grid";
                    Log.Information("✓ Generated {CityCount} cities in grid pattern", cityCount);
                    break;
                default:
                    cities = generator.GenerateRandomCities(cityCount);
                    pattern = "random";
                    Log.Information("✓ Generated {CityCount} random cities", cityCount);
                    break;
            }

            _logger.LogInformation("Generated {CityCount} cities with {Pattern} pattern", cityCount, pattern);

            // Select algorithm
            Log.Information("Select algorithm:");
            Log.Information("  1. Nearest Neighbor (Fast, Good)");
            Log.Information("  2. 2-Opt (Medium, Better)");
            Log.Information("  3. Simulated Annealing (Slow, Very Good)");
            Log.Information("  4. Genetic Algorithm (Slowest, Best)");
            Log.Information("➤ Select algorithm (1-4): ");

            ITspSolver solver;
            var algoChoice = Console.ReadLine();
            switch (algoChoice)
            {
                case "2":
                    solver = new TwoOptSolver(logger: _loggerFactory.CreateLogger<TwoOptSolver>());
                    break;
                case "3":
                    solver = new SimulatedAnnealingSolver(logger: _loggerFactory.CreateLogger<SimulatedAnnealingSolver>());
                    break;
                case "4":
                    solver = GeneticAlgorithmSolver.CreateScaledGeneticSolver(cityCount, logger: _loggerFactory.CreateLogger<GeneticAlgorithmSolver>());
                    break;
                default:
                    solver = new NearestNeighborSolver(_loggerFactory.CreateLogger<NearestNeighborSolver>());
                    break;
            }

            _logger.LogInformation("Selected algorithm: {Algorithm} for {CityCount} cities", solver.Name, cityCount);
            Log.Information("🔄 Running {Algorithm} algorithm...", solver.Name);

            // Setup progress reporting - show dots for visual feedback
            var progressCount = 0;
            solver.ProgressChanged += (s, e) =>
            {
                if (progressCount++ % 10 == 0)
                {
                    Console.Write(".");
                }
            };

            // Solve
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting TSP solution with {Algorithm}", solver.Name);
            
            var tour = await solver.SolveAsync(cities);
            stopwatch.Stop();

            // Log the results
            _logger.LogInformation("TSP solution completed - Algorithm: {Algorithm}, Cities: {CityCount}, Pattern: {Pattern}, " +
                          "Distance: {Distance:F2}, Time: {TimeMs}ms", 
                          solver.Name, cityCount, pattern, tour.TotalDistance, stopwatch.ElapsedMilliseconds);

            // Display results
            Console.WriteLine(); // Clear progress dots
            Console.WriteLine();
            Log.Information(new string('═', 60));
            Log.Information("✓ Solution Found!");
            Log.Information(new string('═', 60));

            Log.Information("Algorithm: {Algorithm}", solver.Name);
            Log.Information("Execution Time: {TimeMs:N0} ms", stopwatch.ElapsedMilliseconds);
            Log.Information("Total Distance: {Distance:F2} units", tour.TotalDistance);
            Log.Information("Route ({CityCount} cities):", tour.Cities.Count);

            var routeStr = string.Join(" → ", tour.Cities.Take(Math.Min(10, tour.Cities.Count)).Select(c => c.Name));
            if (tour.Cities.Count > 10)
            {
                routeStr += " → ... → " + tour.Cities.Last().Name;
            }
            routeStr += " → " + tour.Cities[0].Name;

            Log.Information(routeStr);

            // Show city coordinates if requested
            Log.Information("Show city coordinates? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                Log.Information("City Coordinates:");
                foreach (var city in cities.Take(Math.Min(20, cities.Count)))
                {
                    Log.Information("  {CityName}: ({X:F2}, {Y:F2})", city.Name, city.X, city.Y);
                }
                if (cities.Count > 20)
                {
                    Log.Information("  ... and {AdditionalCount} more cities", cities.Count - 20);
                }
            }

            _logger.LogInformation("Interactive solver session completed successfully");
        }

        static async Task RunBenchmark()
        {
            _logger.LogInformation("Starting benchmark session");
            LogSectionHeader("Algorithm Benchmark");

            Log.Information("Number of cities for benchmark: ");
            if (!int.TryParse(Console.ReadLine(), out int cityCount) || cityCount < 2)
            {
                cityCount = 15;
                Log.Information("Invalid input. Using default of {CityCount} cities.", cityCount);
                _logger.LogWarning("Invalid benchmark city count, using default: {CityCount}", cityCount);
            }

            if (cityCount > 50)
            {
                Log.Warning("⚠️ Benchmark with {CityCount} cities may take several minutes.", cityCount);
                _logger.LogWarning("Large benchmark requested: {CityCount} cities", cityCount);
            }

            var generator = new TspDataGenerator(logger: _loggerFactory.CreateLogger<TspDataGenerator>());
            var cities = generator.GenerateRandomCities(cityCount);

            _logger.LogInformation("Generated {CityCount} random cities for benchmark", cityCount);

            Log.Information("🔄 Running benchmark with {CityCount} cities...", cityCount);
            Log.Information("This may take a moment...");

            var benchmark = new TspBenchmark(_loggerFactory.CreateLogger<TspBenchmark>());
            var solvers = new List<ITspSolver>
            {
                new NearestNeighborSolver(_loggerFactory.CreateLogger<NearestNeighborSolver>()),
                new TwoOptSolver(maxIterations: cityCount * 10, logger: _loggerFactory.CreateLogger<TwoOptSolver>()),
                new SimulatedAnnealingSolver(
                    initialTemperature: cityCount * 100,
                    coolingRate: 0.9995,
                    iterationsPerTemperature: cityCount * 10,
                    logger: _loggerFactory.CreateLogger<SimulatedAnnealingSolver>()),
                new GeneticAlgorithmSolver(
                    populationSize: Math.Max(200, cityCount * 2),
                    generations: Math.Min(5000, cityCount * 20),
                    mutationRate: 0.1,
                    elitismRate: 0.1,
                    logger: _loggerFactory.CreateLogger<GeneticAlgorithmSolver>())
            };

            Console.Write("Processing: ");
            var stopwatch = Stopwatch.StartNew();
            var results = await benchmark.RunBenchmarkAsync(cities, solvers);
            stopwatch.Stop();

            Console.WriteLine(" Done!");

            // Log detailed benchmark results
            _logger.LogInformation("Benchmark completed - Cities: {CityCount}, TotalTime: {TotalTimeMs}ms", 
                cityCount, stopwatch.ElapsedMilliseconds);

            foreach (var result in results)
            {
                _logger.LogInformation("Benchmark result - Algorithm: {Algorithm}, Distance: {Distance:F2}, " +
                              "Time: {TimeMs}ms, Rank: {Rank}", 
                              result.SolverName, result.Distance, result.ExecutionTime.TotalMilliseconds,
                              results.ToList().IndexOf(result) + 1);
            }

            Log.Information(benchmark.FormatResults(results));

            // Display winner details
            var winner = results.First();
            Log.Information("🏆 Winner: {SolverName}", winner.SolverName);
            Log.Information("   Distance: {Distance:F2}", winner.Distance);
            Log.Information("   Time: {TimeMs:F1} ms", winner.ExecutionTime.TotalMilliseconds);

            _logger.LogInformation("Benchmark session completed - Winner: {Winner}, Distance: {Distance:F2}",
                winner.SolverName, winner.Distance);
        }

        static async Task RunDemonstration()
        {
            _logger.LogInformation("Starting demonstration session");
            LogSectionHeader("Visual Algorithm Demonstration");

            Log.Information("This demonstration will show how different algorithms");
            Log.Information("approach the TSP problem step by step.");

            var generator = new TspDataGenerator(42, _loggerFactory.CreateLogger<TspDataGenerator>());
            var cities = generator.GenerateCircularCities(8); // Small number for clarity

            Log.Information("Generated {CityCount} cities in a circular pattern.", cities.Count);
            Log.Information("Cities:");
            foreach (var city in cities)
            {
                Log.Information("  {CityName}: ({X:F1}, {Y:F1})", city.Name, city.X, city.Y);
            }

            Log.Information("Press any key to start the demonstration...");
            Console.ReadKey();
            Console.WriteLine(); // Add newline after keypress

            // Demonstrate each algorithm
            var algorithms = new (string name, ITspSolver solver)[]
            {
                ("Nearest Neighbor", new NearestNeighborSolver(_loggerFactory.CreateLogger<NearestNeighborSolver>())),
                ("2-Opt Improvement", new TwoOptSolver(100, _loggerFactory.CreateLogger<TwoOptSolver>())),
                ("Simulated Annealing", new SimulatedAnnealingSolver(1000, 0.99, 50, logger: _loggerFactory.CreateLogger<SimulatedAnnealingSolver>())),
                ("Genetic Algorithm", new GeneticAlgorithmSolver(20, 50, 0.05, 0.3, logger: _loggerFactory.CreateLogger<GeneticAlgorithmSolver>()))
            };

            foreach (var (name, solver) in algorithms)
            {
                _logger.LogInformation("Running demonstration for {Algorithm}", name);
                Log.Information(new string('─', 60));
                LogSectionHeader($"Algorithm: {name}");

                Log.Information("🔄 Running {Algorithm}...", name);

                var iterations = new List<(int iteration, double distance, string message)>();
                solver.ProgressChanged += (s, e) =>
                {
                    iterations.Add((e.Iteration, e.CurrentBestDistance, e.Message));
                };

                var tour = await solver.SolveAsync(cities);

                // Display progress summary
                if (iterations.Count > 0)
                {
                    Log.Information("Algorithm Progress Summary:");
                    Log.Information(new string('-', 50));

                    var first = iterations.First();
                    var last = iterations.Last();
                    var best = iterations.MinBy(i => i.distance);

                    Log.Information("  Initial: Distance = {Distance:F2}", first.distance);
                    if (iterations.Count > 2)
                    {
                        Log.Information("  Best:    Distance = {Distance:F2} (at iteration {Iteration})", best.distance, best.iteration);
                    }
                    Log.Information("  Final:   Distance = {Distance:F2}", last.distance);
                    Log.Information("  Total iterations: {IterationCount}", iterations.Count);
                }

                Log.Information(new string('-', 50));
                Log.Information("✓ Final Solution:");
                Log.Information("  Distance: {Distance:F2}", tour.TotalDistance);
                Log.Information("  Route: {Route} → {FirstCity}", string.Join(" → ", tour.Cities.Select(c => c.Name)), tour.Cities[0].Name);

                DrawSimpleVisualization(tour);

                if (algorithms.Last() != (name, solver))
                {
                    Log.Information("Press any key for next algorithm...");
                    Console.ReadKey();
                    Console.WriteLine(); // Add newline after keypress
                }
            }

            Log.Information(new string('═', 60));
            Log.Information("Demonstration Complete! All algorithms have been demonstrated.");
            _logger.LogInformation("Demonstration session completed successfully");
        }

        static void ShowAlgorithmInfo()
        {
            _logger.LogInformation("Displaying algorithm information");
            LogSectionHeader("Algorithm Information");

            var info = new Dictionary<string, (string complexity, string pros, string cons, string description)>
            {
                ["Nearest Neighbor"] = (
                    "O(n²)",
                    "Fast, simple, deterministic",
                    "Can produce suboptimal solutions",
                    "Builds tour by always visiting the nearest unvisited city."
                ),
                ["2-Opt"] = (
                    "O(n²) per iteration",
                    "Good improvement over initial solution",
                    "Can get stuck in local optima",
                    "Improves existing tour by reversing segments to reduce crossings."
                ),
                ["Simulated Annealing"] = (
                    "O(n) per iteration × iterations",
                    "Can escape local optima, tunable parameters",
                    "Slower, non-deterministic",
                    "Uses controlled randomness to explore solution space, accepting worse solutions probabilistically."
                ),
                ["Genetic Algorithm"] = (
                    "O(p×g×n) where p=population, g=generations",
                    "Excellent for large problems, parallelizable",
                    "Slowest, many parameters to tune",
                    "Evolves population of solutions using selection, crossover, and mutation."
                )
            };

            foreach (var (algo, (complexity, pros, cons, description)) in info)
            {
                Log.Information("📍 {Algorithm}", algo);
                Log.Information(new string('-', 40));
                Log.Information("Description: {Description}", description);
                Log.Information("Complexity:  {Complexity}", complexity);
                Log.Information("Pros:        {Pros}", pros);
                Log.Information("Cons:        {Cons}", cons);
            }

            Log.Information(new string('═', 60));
            Log.Information("💡 Recommendations:");
            Log.Information("  • Small problems (< 20 cities): Nearest Neighbor + 2-Opt");
            Log.Information("  • Medium problems (20-100 cities): Simulated Annealing");
            Log.Information("  • Large problems (> 100 cities): Genetic Algorithm");
            Log.Information("  • Real-time requirements: Nearest Neighbor");
            Log.Information("  • Best quality: Genetic Algorithm with tuned parameters");
        }

        static void DrawSimpleVisualization(Tour tour)
        {
            Log.Information("Simple ASCII Visualization:");
            Log.Information(new string('─', 50));

            const int width = 40;
            const int height = 10;
            var grid = new char[height, width];

            // Initialize grid
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    grid[i, j] = ' ';

            // Normalize coordinates to fit grid
            var minX = tour.Cities.Min(c => c.X);
            var maxX = tour.Cities.Max(c => c.X);
            var minY = tour.Cities.Min(c => c.Y);
            var maxY = tour.Cities.Max(c => c.Y);

            foreach (var city in tour.Cities)
            {
                var x = (int)((city.X - minX) / (maxX - minX) * (width - 1));
                var y = (int)((city.Y - minY) / (maxY - minY) * (height - 1));

                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    grid[height - 1 - y, x] = '●';
                }
            }

            // Draw grid
            for (int i = 0; i < height; i++)
            {
                var line = "  ";
                for (int j = 0; j < width; j++)
                {
                    line += grid[i, j];
                }
                Log.Information(line);
            }

            Log.Information(new string('─', 50));
        }

        static void PrintHeader()
        {
            Console.WriteLine(); // Space from previous output
            Log.Information("╔═══════════════════════════════════════════════════════════════╗");
            Log.Information("║          TRAVELING SALESMAN PROBLEM SOLVER v{Version,-12} ║", GetAssemblyVersion());
            Log.Information("║                  .NET 9 Implementation                        ║");
            Log.Information("╚═══════════════════════════════════════════════════════════════╝");
        }

        static int ShowMainMenu()
        {
            Log.Information("📍 Main Menu:");
            Log.Information("");
            Log.Information("  1. Interactive Solver - Solve custom TSP instances");
            Log.Information("  2. Algorithm Benchmark - Compare all algorithms");
            Log.Information("  3. Visual Demonstration - See algorithms in action");
            Log.Information("  4. Algorithm Information - Learn about each algorithm");
            Log.Information("  5. Exit");

            Log.Information("➤ Select an option (1-5): ");

            if (int.TryParse(Console.ReadLine(), out int option))
            {
                return option;
            }

            return -1;
        }

        static void LogSectionHeader(string title)
        {
            Log.Information(new string('═', 60));
            Log.Information("  {Title}", title);
            Log.Information(new string('═', 60));
        }

        static string GetAssemblyVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version?.ToString(3) ?? "1.2.0";
        }
    }
}