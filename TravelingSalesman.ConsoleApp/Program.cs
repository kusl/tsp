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
                            LogToConsole("\nThank you for using TSP Solver! Goodbye!");
                            return;
                        default:
                            _logger.LogWarning("Invalid menu option selected: {Option}", option);
                            LogError("Invalid option. Please try again.");
                            break;
                    }

                    if (option != 5)
                    {
                        LogToConsole("\nPress any key to return to main menu...");
                        Console.ReadKey();
                        LogToConsole(""); // Add newline after keypress
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogFatal(ex, "Unexpected error occurred in main application loop");
                LogError($"An unexpected error occurred: {ex.Message}");
                LogToConsole("\nPress any key to exit...");
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
            LogToConsole("\nHow many cities? (minimum 2): ");
            if (!int.TryParse(Console.ReadLine(), out int cityCount) || cityCount < 2)
            {
                _logger.LogWarning("Invalid city count input, using default of 10");
                LogError("Invalid input. Using default of 10 cities.");
                cityCount = 10;
            }

            _logger.LogInformation("Interactive solver configured for {CityCount} cities", cityCount);

            // Warn for large numbers
            if (cityCount > 100)
            {
                _logger.LogWarning("Large city count requested: {CityCount}", cityCount);
                LogWarning($"\n⚠️ Note: {cityCount} cities may take significant time with some algorithms.");
                LogWarning("   Nearest Neighbor will be fast, but Genetic Algorithm may take minutes.");
                LogToConsole("   Continue? (y/n): ");

                if (Console.ReadLine()?.ToLower() != "y")
                {
                    LogToConsole("Using 50 cities instead.");
                    cityCount = 50;
                    _logger.LogInformation("User reduced city count to {CityCount}", cityCount);
                }
            }

            // Select data pattern
            LogToConsole("\nSelect city distribution pattern:");
            LogToConsole("  1. Random");
            LogToConsole("  2. Circular");
            LogToConsole("  3. Grid");
            LogToConsole("\n➤ Select pattern (1-3): ");

            var generator = new TspDataGenerator(42, _loggerFactory.CreateLogger<TspDataGenerator>());
            IReadOnlyList<City> cities;
            string pattern;

            var patternChoice = Console.ReadLine();
            switch (patternChoice)
            {
                case "2":
                    cities = generator.GenerateCircularCities(cityCount);
                    pattern = "circular";
                    LogSuccess($"\n✓ Generated {cityCount} cities in circular pattern");
                    break;
                case "3":
                    var gridSize = (int)Math.Sqrt(cityCount);
                    cities = generator.GenerateGridCities(gridSize, gridSize + (cityCount - gridSize * gridSize) / gridSize + 1);
                    cities = cities.Take(cityCount).ToList();
                    pattern = "grid";
                    LogSuccess($"\n✓ Generated {cityCount} cities in grid pattern");
                    break;
                default:
                    cities = generator.GenerateRandomCities(cityCount);
                    pattern = "random";
                    LogSuccess($"\n✓ Generated {cityCount} random cities");
                    break;
            }

            _logger.LogInformation("Generated {CityCount} cities with {Pattern} pattern", cityCount, pattern);

            // Select algorithm
            LogToConsole("\nSelect algorithm:");
            LogToConsole("  1. Nearest Neighbor (Fast, Good)");
            LogToConsole("  2. 2-Opt (Medium, Better)");
            LogToConsole("  3. Simulated Annealing (Slow, Very Good)");
            LogToConsole("  4. Genetic Algorithm (Slowest, Best)");
            LogToConsole("\n➤ Select algorithm (1-4): ");

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
            LogToConsole($"\n🔄 Running {solver.Name} algorithm...\n");

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
            LogToConsole("\n\n" + new string('═', 60));
            LogSuccess("✓ Solution Found!");
            LogToConsole(new string('═', 60));

            LogToConsole($"\nAlgorithm: {solver.Name}");
            LogToConsole($"Execution Time: {stopwatch.ElapsedMilliseconds:N0} ms");
            LogToConsole($"Total Distance: {tour.TotalDistance:F2} units");
            LogToConsole($"\nRoute ({tour.Cities.Count} cities):");

            var routeStr = string.Join(" → ", tour.Cities.Take(Math.Min(10, tour.Cities.Count)).Select(c => c.Name));
            if (tour.Cities.Count > 10)
            {
                routeStr += " → ... → " + tour.Cities.Last().Name;
            }
            routeStr += " → " + tour.Cities[0].Name;

            LogToConsole(routeStr);

            // Show city coordinates if requested
            LogToConsole("\nShow city coordinates? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                LogToConsole("\nCity Coordinates:");
                foreach (var city in cities.Take(Math.Min(20, cities.Count)))
                {
                    LogToConsole($"  {city.Name}: ({city.X:F2}, {city.Y:F2})");
                }
                if (cities.Count > 20)
                {
                    LogToConsole($"  ... and {cities.Count - 20} more cities");
                }
            }

            _logger.LogInformation("Interactive solver session completed successfully");
        }

        static async Task RunBenchmark()
        {
            _logger.LogInformation("Starting benchmark session");
            LogSectionHeader("Algorithm Benchmark");

            LogToConsole("\nNumber of cities for benchmark: ");
            if (!int.TryParse(Console.ReadLine(), out int cityCount) || cityCount < 2)
            {
                cityCount = 15;
                LogToConsole($"Invalid input. Using default of {cityCount} cities.");
                _logger.LogWarning("Invalid benchmark city count, using default: {CityCount}", cityCount);
            }

            if (cityCount > 50)
            {
                LogWarning($"\n⚠️ Benchmark with {cityCount} cities may take several minutes.");
                _logger.LogWarning("Large benchmark requested: {CityCount} cities", cityCount);
            }

            var generator = new TspDataGenerator(logger: _loggerFactory.CreateLogger<TspDataGenerator>());
            var cities = generator.GenerateRandomCities(cityCount);

            _logger.LogInformation("Generated {CityCount} random cities for benchmark", cityCount);

            LogToConsole($"\n🔄 Running benchmark with {cityCount} cities...\n");
            LogToConsole("This may take a moment...\n");

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

            LogToConsole("Processing: ");
            var stopwatch = Stopwatch.StartNew();
            var results = await benchmark.RunBenchmarkAsync(cities, solvers);
            stopwatch.Stop();

            LogToConsole(" Done!");

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

            LogToConsole(benchmark.FormatResults(results));

            // Display winner details
            var winner = results.First();
            LogSuccess($"\n🏆 Winner: {winner.SolverName}");
            LogToConsole($"   Distance: {winner.Distance:F2}");
            LogToConsole($"   Time: {winner.ExecutionTime.TotalMilliseconds:F1} ms");

            // Show relative performance
            if (results.Count > 1)
            {
                LogToConsole("\n📊 Relative Performance:");
                var maxBarLength = 40;
                var bestTime = results.Min(r => r.ExecutionTime.TotalMilliseconds);
                var bestDistance = results.Min(r => r.Distance);

                foreach (var result in results)
                {
                    var distanceRatio = result.Distance / bestDistance;
                    var timeRatio = result.ExecutionTime.TotalMilliseconds / bestTime;

                    var distanceBar = new string('█', (int)(maxBarLength / distanceRatio));
                    var timeBar = new string('█', Math.Min(maxBarLength, (int)(maxBarLength / timeRatio)));

                    LogToConsole($"\n  {result.SolverName}:");
                    LogToConsole("    Distance: ", GetColorForRatio(distanceRatio));
                    LogToConsole(distanceBar);
                    Console.ResetColor();

                    LogToConsole("    Speed:    ", GetColorForRatio(timeRatio));
                    LogToConsole(timeBar);
                    Console.ResetColor();
                }
            }

            _logger.LogInformation("Benchmark session completed - Winner: {Winner}, Distance: {Distance:F2}",
                winner.SolverName, winner.Distance);
        }

        static async Task RunDemonstration()
        {
            _logger.LogInformation("Starting demonstration session");
            LogSectionHeader("Visual Algorithm Demonstration");

            LogToConsole("\nThis demonstration will show how different algorithms");
            LogToConsole("approach the TSP problem step by step.\n");

            var generator = new TspDataGenerator(42, _loggerFactory.CreateLogger<TspDataGenerator>());
            var cities = generator.GenerateCircularCities(8); // Small number for clarity

            LogToConsole($"Generated {cities.Count} cities in a circular pattern.\n");
            LogToConsole("Cities:");
            foreach (var city in cities)
            {
                LogToConsole($"  {city.Name}: ({city.X:F1}, {city.Y:F1})");
            }

            LogToConsole("\nPress any key to start the demonstration...");
            Console.ReadKey();
            LogToConsole(""); // Add newline after keypress

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
                LogToConsole("\n" + new string('─', 60));
                LogSectionHeader($"Algorithm: {name}");

                LogToConsole($"\n🔄 Running {name}...\n");

                var iterations = new List<(int iteration, double distance, string message)>();
                solver.ProgressChanged += (s, e) =>
                {
                    iterations.Add((e.Iteration, e.CurrentBestDistance, e.Message));
                };

                var tour = await solver.SolveAsync(cities);

                // Display progress summary
                if (iterations.Count > 0)
                {
                    LogToConsole("Algorithm Progress Summary:");
                    LogToConsole(new string('-', 50));

                    var first = iterations.First();
                    var last = iterations.Last();
                    var best = iterations.MinBy(i => i.distance);

                    LogToConsole($"  Initial: Distance = {first.distance:F2}");
                    if (iterations.Count > 2)
                    {
                        LogToConsole($"  Best:    Distance = {best.distance:F2} (at iteration {best.iteration})");
                    }
                    LogToConsole($"  Final:   Distance = {last.distance:F2}");
                    LogToConsole($"  Total iterations: {iterations.Count}");
                }

                LogToConsole(new string('-', 50));
                LogToConsole($"\n✓ Final Solution:");
                LogToConsole($"  Distance: {tour.TotalDistance:F2}");
                LogToConsole($"  Route: {string.Join(" → ", tour.Cities.Select(c => c.Name))} → {tour.Cities[0].Name}");

                DrawSimpleVisualization(tour);

                if (algorithms.Last() != (name, solver))
                {
                    LogToConsole("\nPress any key for next algorithm...");
                    Console.ReadKey();
                    LogToConsole(""); // Add newline after keypress
                }
            }

            LogToConsole("\n" + new string('═', 60));
            LogSuccess("Demonstration Complete! All algorithms have been demonstrated.");
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
                LogInfo($"\n📍 {algo}");
                LogToConsole(new string('-', 40));
                LogToConsole($"Description: {description}");
                LogToConsole($"Complexity:  {complexity}");
                LogSuccess($"Pros:        {pros}");
                LogWarning($"Cons:        {cons}");
            }

            LogToConsole("\n" + new string('═', 60));
            LogToConsole("\n💡 Recommendations:");
            LogToConsole("  • Small problems (< 20 cities): Nearest Neighbor + 2-Opt");
            LogToConsole("  • Medium problems (20-100 cities): Simulated Annealing");
            LogToConsole("  • Large problems (> 100 cities): Genetic Algorithm");
            LogToConsole("  • Real-time requirements: Nearest Neighbor");
            LogToConsole("  • Best quality: Genetic Algorithm with tuned parameters");
        }

        static void DrawSimpleVisualization(Tour tour)
        {
            LogToConsole("\nSimple ASCII Visualization:");
            LogToConsole(new string('─', 50));

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
                LogToConsole("  ");
                for (int j = 0; j < width; j++)
                {
                    if (grid[i, j] == '●')
                    {
                        LogToConsole(grid[i, j].ToString(), ConsoleColor.Red);
                    }
                    else
                    {
                        LogToConsole(grid[i, j].ToString());
                    }
                }
                LogToConsole("");
            }

            LogToConsole(new string('─', 50));
        }

        static void PrintHeader()
        {
            LogToConsole(""); // Space from previous output
            LogInfo("╔═══════════════════════════════════════════════════════════════╗");
            LogInfo("║          TRAVELING SALESMAN PROBLEM SOLVER v" + GetAssemblyVersion().PadRight(12) + " ║");
            LogInfo("║                  .NET 9 Implementation                        ║");
            LogInfo("╚═══════════════════════════════════════════════════════════════╝");
        }

        static int ShowMainMenu()
        {
            LogToConsole("\n📍 Main Menu:\n");
            LogToConsole("  1. Interactive Solver - Solve custom TSP instances");
            LogToConsole("  2. Algorithm Benchmark - Compare all algorithms");
            LogToConsole("  3. Visual Demonstration - See algorithms in action");
            LogToConsole("  4. Algorithm Information - Learn about each algorithm");
            LogToConsole("  5. Exit");

            LogToConsole("\n➤ Select an option (1-5): ");

            if (int.TryParse(Console.ReadLine(), out int option))
            {
                return option;
            }

            return -1;
        }

        static void LogSectionHeader(string title)
        {
            LogInfo("\n" + new string('═', 60));
            LogInfo($"  {title}");
            LogInfo(new string('═', 60));
        }

        static void LogToConsole(string message, ConsoleColor? color = null)
        {
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }
            Console.Write(message);
            if (color.HasValue)
            {
                Console.ResetColor();
            }
        }

        static void LogSuccess(string message)
        {
            LogToConsole(message, ConsoleColor.Green);
        }

        static void LogError(string message)
        {
            LogToConsole($"\n❌ Error: {message}", ConsoleColor.Red);
        }

        static void LogWarning(string message)
        {
            LogToConsole(message, ConsoleColor.Yellow);
        }

        static void LogInfo(string message