using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TravelingSalesman.Core;
using Serilog;

namespace TravelingSalesman.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/tsp-solver-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .Enrich.WithProperty("Application", "TSP-Solver")
                .CreateLogger();

            Log.Information("TSP Solver starting up");

            try
            {
                PrintHeader();

                while (true)
                {
                    var option = ShowMainMenu();
                    Log.Debug("User selected menu option: {Option}", option);

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
                            Log.Information("User requested exit");
                            Console.WriteLine("\nThank you for using TSP Solver! Goodbye!");
                            return;
                        default:
                            Log.Warning("Invalid menu option selected: {Option}", option);
                            PrintError("Invalid option. Please try again.");
                            break;
                    }

                    if (option != 5)
                    {
                        Console.WriteLine("\nPress any key to return to main menu...");
                        Console.ReadKey();
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unexpected error occurred in main application loop");
                PrintError($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            finally
            {
                Log.Information("TSP Solver shutting down");
                Log.CloseAndFlush();
            }
        }

        static async Task RunInteractiveSolver()
        {
            Log.Information("Starting interactive solver session");
            PrintSectionHeader("Interactive TSP Solver");

            // Get number of cities
            Console.Write("\nHow many cities? (minimum 2): ");
            if (!int.TryParse(Console.ReadLine(), out int cityCount) || cityCount < 2)
            {
                Log.Warning("Invalid city count input, using default of 10");
                PrintError("Invalid input. Using default of 10 cities.");
                cityCount = 10;
            }

            Log.Information("Interactive solver configured for {CityCount} cities", cityCount);

            // Warn for large numbers
            if (cityCount > 100)
            {
                Log.Warning("Large city count requested: {CityCount}", cityCount);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n⚠️ Note: {cityCount} cities may take significant time with some algorithms.");
                Console.WriteLine("   Nearest Neighbor will be fast, but Genetic Algorithm may take minutes.");
                Console.Write("   Continue? (y/n): ");
                Console.ResetColor();

                if (Console.ReadLine()?.ToLower() != "y")
                {
                    Console.WriteLine("Using 50 cities instead.");
                    cityCount = 50;
                    Log.Information("User reduced city count to {CityCount}", cityCount);
                }
            }

            // Select data pattern
            Console.WriteLine("\nSelect city distribution pattern:");
            Console.WriteLine("  1. Random");
            Console.WriteLine("  2. Circular");
            Console.WriteLine("  3. Grid");
            Console.Write("\n➤ Select pattern (1-3): ");

            var generator = new TspDataGenerator(42);
            IReadOnlyList<City> cities;
            string pattern;

            var patternChoice = Console.ReadLine();
            switch (patternChoice)
            {
                case "2":
                    cities = generator.GenerateCircularCities(cityCount);
                    pattern = "circular";
                    Console.WriteLine($"\n✓ Generated {cityCount} cities in circular pattern");
                    break;
                case "3":
                    var gridSize = (int)Math.Sqrt(cityCount);
                    cities = generator.GenerateGridCities(gridSize, gridSize + (cityCount - gridSize * gridSize) / gridSize + 1);
                    cities = cities.Take(cityCount).ToList();
                    pattern = "grid";
                    Console.WriteLine($"\n✓ Generated {cityCount} cities in grid pattern");
                    break;
                default:
                    cities = generator.GenerateRandomCities(cityCount);
                    pattern = "random";
                    Console.WriteLine($"\n✓ Generated {cityCount} random cities");
                    break;
            }

            Log.Information("Generated {CityCount} cities with {Pattern} pattern", cityCount, pattern);

            // Select algorithm
            Console.WriteLine("\nSelect algorithm:");
            Console.WriteLine("  1. Nearest Neighbor (Fast, Good)");
            Console.WriteLine("  2. 2-Opt (Medium, Better)");
            Console.WriteLine("  3. Simulated Annealing (Slow, Very Good)");
            Console.WriteLine("  4. Genetic Algorithm (Slowest, Best)");
            Console.Write("\n➤ Select algorithm (1-4): ");

            ITspSolver solver;
            var algoChoice = Console.ReadLine();
            switch (algoChoice)
            {
                case "2":
                    solver = new TwoOptSolver();
                    break;
                case "3":
                    solver = new SimulatedAnnealingSolver();
                    break;
                case "4":
                    solver = GeneticAlgorithmSolver.CreateScaledGeneticSolver(cityCount);
                    break;
                default:
                    solver = new NearestNeighborSolver();
                    break;
            }

            Log.Information("Selected algorithm: {Algorithm} for {CityCount} cities", solver.Name, cityCount);
            Console.WriteLine($"\n🔄 Running {solver.Name} algorithm...\n");

            // Setup progress reporting
            var progressCount = 0;
            solver.ProgressChanged += (s, e) =>
            {
                if (progressCount++ % 10 == 0)
                {
                    Console.Write(".");
                }
                // Log every 50th iteration to avoid log spam
                if (e.Iteration % 50 == 0)
                {
                    Log.Debug("Algorithm progress - Iteration: {Iteration}, Distance: {Distance:F2}", 
                        e.Iteration, e.CurrentBestDistance);
                }
            };

            // Solve
            var stopwatch = Stopwatch.StartNew();
            Log.Information("Starting TSP solution with {Algorithm}", solver.Name);
            
            var tour = await solver.SolveAsync(cities);
            stopwatch.Stop();

            // Log the results
            Log.Information("TSP solution completed - Algorithm: {Algorithm}, Cities: {CityCount}, Pattern: {Pattern}, " +
                          "Distance: {Distance:F2}, Time: {TimeMs}ms", 
                          solver.Name, cityCount, pattern, tour.TotalDistance, stopwatch.ElapsedMilliseconds);

            // Display results
            Console.WriteLine("\n\n" + new string('═', 60));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Solution Found!");
            Console.ResetColor();
            Console.WriteLine(new string('═', 60));

            Console.WriteLine($"\nAlgorithm: {solver.Name}");
            Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds:N0} ms");
            Console.WriteLine($"Total Distance: {tour.TotalDistance:F2} units");
            Console.WriteLine($"\nRoute ({tour.Cities.Count} cities):");

            var routeStr = string.Join(" → ", tour.Cities.Take(Math.Min(10, tour.Cities.Count)).Select(c => c.Name));
            if (tour.Cities.Count > 10)
            {
                routeStr += " → ... → " + tour.Cities.Last().Name;
            }
            routeStr += " → " + tour.Cities[0].Name;

            Console.WriteLine(routeStr);

            // Show city coordinates if requested
            Console.Write("\nShow city coordinates? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                Console.WriteLine("\nCity Coordinates:");
                foreach (var city in cities.Take(Math.Min(20, cities.Count)))
                {
                    Console.WriteLine($"  {city.Name}: ({city.X:F2}, {city.Y:F2})");
                }
                if (cities.Count > 20)
                {
                    Console.WriteLine($"  ... and {cities.Count - 20} more cities");
                }
            }

            Log.Information("Interactive solver session completed successfully");
        }

        static async Task RunBenchmark()
        {
            Log.Information("Starting benchmark session");
            PrintSectionHeader("Algorithm Benchmark");

            Console.Write("\nNumber of cities for benchmark: ");
            if (!int.TryParse(Console.ReadLine(), out int cityCount) || cityCount < 2)
            {
                cityCount = 15;
                Console.WriteLine($"Invalid input. Using default of {cityCount} cities.");
                Log.Warning("Invalid benchmark city count, using default: {CityCount}", cityCount);
            }

            if (cityCount > 50)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n⚠️ Benchmark with {cityCount} cities may take several minutes.");
                Console.ResetColor();
                Log.Warning("Large benchmark requested: {CityCount} cities", cityCount);
            }

            var generator = new TspDataGenerator();
            var cities = generator.GenerateRandomCities(cityCount);

            Log.Information("Generated {CityCount} random cities for benchmark", cityCount);

            Console.WriteLine($"\n🔄 Running benchmark with {cityCount} cities...\n");
            Console.WriteLine("This may take a moment...\n");

            var benchmark = new TspBenchmark();
            var solvers = new List<ITspSolver>
            {
                new NearestNeighborSolver(),
                new TwoOptSolver(maxIterations: cityCount * 10),
                new SimulatedAnnealingSolver(
                    initialTemperature: cityCount * 100,
                    coolingRate: 0.9995,
                    iterationsPerTemperature: cityCount * 10),
                new GeneticAlgorithmSolver(
                    populationSize: Math.Max(200, cityCount * 2),
                    generations: Math.Min(5000, cityCount * 20),
                    mutationRate: 0.1,
                    elitismRate: 0.1)
            };

            Console.Write("Processing: ");
            var stopwatch = Stopwatch.StartNew();
            var results = await benchmark.RunBenchmarkAsync(cities, solvers);
            stopwatch.Stop();

            Console.WriteLine(" Done!");

            // Log detailed benchmark results
            Log.Information("Benchmark completed - Cities: {CityCount}, TotalTime: {TotalTimeMs}ms", 
                cityCount, stopwatch.ElapsedMilliseconds);

            foreach (var result in results)
            {
                Log.Information("Benchmark result - Algorithm: {Algorithm}, Distance: {Distance:F2}, " +
                              "Time: {TimeMs}ms, Rank: {Rank}", 
                              result.SolverName, result.Distance, result.ExecutionTime.TotalMilliseconds,
                              results.ToList().IndexOf(result) + 1);
            }

            Console.WriteLine(benchmark.FormatResults(results));

            // Display winner details
            var winner = results.First();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n🏆 Winner: {winner.SolverName}");
            Console.ResetColor();
            Console.WriteLine($"   Distance: {winner.Distance:F2}");
            Console.WriteLine($"   Time: {winner.ExecutionTime.TotalMilliseconds:F1} ms");

            Log.Information("Benchmark session completed - Winner: {Winner}, Distance: {Distance:F2}",
                winner.SolverName, winner.Distance);
        }

        // ... rest of the methods remain the same ...

        static void PrintHeader()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          TRAVELING SALESMAN PROBLEM SOLVER v1.0               ║");
            Console.WriteLine("║                  .NET 9 Implementation                        ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        static int ShowMainMenu()
        {
            Console.WriteLine("\n📍 Main Menu:\n");
            Console.WriteLine("  1. Interactive Solver - Solve custom TSP instances");
            Console.WriteLine("  2. Algorithm Benchmark - Compare all algorithms");
            Console.WriteLine("  3. Visual Demonstration - See algorithms in action");
            Console.WriteLine("  4. Algorithm Information - Learn about each algorithm");
            Console.WriteLine("  5. Exit");

            Console.Write("\n➤ Select an option (1-5): ");

            if (int.TryParse(Console.ReadLine(), out int option))
            {
                return option;
            }

            return -1;
        }

        // Other methods remain the same but would benefit from logging at key points...
        
        static void PrintSectionHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine($"  {title}");
            Console.WriteLine(new string('═', 60));
            Console.ResetColor();
        }

        static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Error: {message}");
            Console.ResetColor();
        }
    }
}
