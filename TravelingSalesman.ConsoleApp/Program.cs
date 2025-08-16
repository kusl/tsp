using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TravelingSalesman.Core;

namespace TravelingSalesman.ConsoleApp
{
    class Program
    {
        private static readonly ConsoleColor[] _colors =
        {
            ConsoleColor.Cyan,
            ConsoleColor.Green,
            ConsoleColor.Yellow,
            ConsoleColor.Magenta
        };

        static async Task Main(string[] args)
        {
            // Don't set Console.Title - it's intrusive
            PrintHeader();

            try
            {
                while (true)
                {
                    var option = ShowMainMenu();

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
                            Console.WriteLine("\nThank you for using TSP Solver! Goodbye!");
                            return;
                        default:
                            PrintError("Invalid option. Please try again.");
                            break;
                    }

                    if (option != 5)
                    {
                        Console.WriteLine("\nPress any key to return to main menu...");
                        Console.ReadKey();
                        Console.WriteLine(); // Add newline after keypress
                    }
                }
            }
            catch (Exception ex)
            {
                PrintError($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        static void PrintHeader()
        {
            Console.WriteLine(); // Space from previous output
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

        static async Task RunInteractiveSolver()
        {
            PrintSectionHeader("Interactive TSP Solver");

            // Get number of cities
            Console.Write("\nHow many cities? (4-50): ");
            if (!int.TryParse(Console.ReadLine(), out int cityCount) || cityCount < 4 || cityCount > 50)
            {
                PrintError("Invalid input. Using default of 10 cities.");
                cityCount = 10;
            }

            // Select data pattern
            Console.WriteLine("\nSelect city distribution pattern:");
            Console.WriteLine("  1. Random");
            Console.WriteLine("  2. Circular");
            Console.WriteLine("  3. Grid");
            Console.Write("\n➤ Select pattern (1-3): ");

            var generator = new TspDataGenerator(42); // Fixed seed for reproducibility
            IReadOnlyList<City> cities;

            var patternChoice = Console.ReadLine();
            switch (patternChoice)
            {
                case "2":
                    cities = generator.GenerateCircularCities(cityCount);
                    Console.WriteLine($"\n✓ Generated {cityCount} cities in circular pattern");
                    break;
                case "3":
                    var gridSize = (int)Math.Sqrt(cityCount);
                    cities = generator.GenerateGridCities(gridSize, gridSize + (cityCount - gridSize * gridSize) / gridSize + 1);
                    cities = cities.Take(cityCount).ToList();
                    Console.WriteLine($"\n✓ Generated {cityCount} cities in grid pattern");
                    break;
                default:
                    cities = generator.GenerateRandomCities(cityCount);
                    Console.WriteLine($"\n✓ Generated {cityCount} random cities");
                    break;
            }

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
                    solver = new GeneticAlgorithmSolver();
                    break;
                default:
                    solver = new NearestNeighborSolver();
                    break;
            }

            Console.WriteLine($"\n🔄 Running {solver.Name} algorithm...\n");

            // Setup progress reporting - use simple dots instead of overwriting
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
            var tour = await solver.SolveAsync(cities);
            stopwatch.Stop();

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
        }

        static async Task RunBenchmark()
        {
            PrintSectionHeader("Algorithm Benchmark");

            Console.Write("\nNumber of cities for benchmark (10-30): ");
            if (!int.TryParse(Console.ReadLine(), out int cityCount) || cityCount < 10 || cityCount > 30)
            {
                cityCount = 15;
                Console.WriteLine($"Using default of {cityCount} cities.");
            }

            var generator = new TspDataGenerator(42);
            var cities = generator.GenerateRandomCities(cityCount);

            Console.WriteLine($"\n🔄 Running benchmark with {cityCount} cities...\n");
            Console.WriteLine("This may take a moment...\n");

            var benchmark = new TspBenchmark();
            var solvers = TspSolverFactory.CreateAllSolvers();

            Console.Write("Processing: ");
            var results = await benchmark.RunBenchmarkAsync(cities, solvers);
            Console.WriteLine(" Done!");

            Console.WriteLine(benchmark.FormatResults(results));

            // Display winner details
            var winner = results.First();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n🏆 Winner: {winner.SolverName}");
            Console.ResetColor();
            Console.WriteLine($"   Distance: {winner.Distance:F2}");
            Console.WriteLine($"   Time: {winner.ExecutionTime.TotalMilliseconds:F1} ms");

            // Show relative performance
            if (results.Count > 1)
            {
                Console.WriteLine("\n📊 Relative Performance:");
                var maxBarLength = 40;
                var bestTime = results.Min(r => r.ExecutionTime.TotalMilliseconds);
                var bestDistance = results.Min(r => r.Distance);

                foreach (var result in results)
                {
                    var distanceRatio = result.Distance / bestDistance;
                    var timeRatio = result.ExecutionTime.TotalMilliseconds / bestTime;

                    var distanceBar = new string('█', (int)(maxBarLength / distanceRatio));
                    var timeBar = new string('█', Math.Min(maxBarLength, (int)(maxBarLength / timeRatio)));

                    Console.WriteLine($"\n  {result.SolverName}:");
                    Console.Write("    Distance: ");
                    Console.ForegroundColor = GetColorForRatio(distanceRatio);
                    Console.WriteLine(distanceBar);
                    Console.ResetColor();

                    Console.Write("    Speed:    ");
                    Console.ForegroundColor = GetColorForRatio(timeRatio);
                    Console.WriteLine(timeBar);
                    Console.ResetColor();
                }
            }
        }

        static async Task RunDemonstration()
        {
            PrintSectionHeader("Visual Algorithm Demonstration");

            Console.WriteLine("\nThis demonstration will show how different algorithms");
            Console.WriteLine("approach the TSP problem step by step.\n");

            var generator = new TspDataGenerator(42);
            var cities = generator.GenerateCircularCities(8); // Small number for clarity

            Console.WriteLine($"Generated {cities.Count} cities in a circular pattern.\n");
            Console.WriteLine("Cities:");
            foreach (var city in cities)
            {
                Console.WriteLine($"  {city.Name}: ({city.X:F1}, {city.Y:F1})");
            }

            Console.WriteLine("\nPress any key to start the demonstration...");
            Console.ReadKey();
            Console.WriteLine(); // Add newline after keypress

            // Demonstrate each algorithm
            var algorithms = new (string name, ITspSolver solver)[]
            {
                ("Nearest Neighbor", new NearestNeighborSolver()),
                ("2-Opt Improvement", new TwoOptSolver(100)),
                ("Simulated Annealing", new SimulatedAnnealingSolver(1000, 0.99, 50)),
                ("Genetic Algorithm", new GeneticAlgorithmSolver(20, 50, 0.05, 0.3))
            };

            foreach (var (name, solver) in algorithms)
            {
                Console.WriteLine("\n" + new string('─', 60));
                PrintSectionHeader($"Algorithm: {name}");

                Console.WriteLine($"\n🔄 Running {name}...\n");

                var iterations = new List<(int iteration, double distance, string message)>();
                solver.ProgressChanged += (s, e) =>
                {
                    iterations.Add((e.Iteration, e.CurrentBestDistance, e.Message));
                };

                var tour = await solver.SolveAsync(cities);

                // Display progress summary (not all iterations)
                if (iterations.Count > 0)
                {
                    Console.WriteLine("Algorithm Progress Summary:");
                    Console.WriteLine(new string('-', 50));

                    var first = iterations.First();
                    var last = iterations.Last();
                    var best = iterations.MinBy(i => i.distance);

                    Console.WriteLine($"  Initial: Distance = {first.distance:F2}");
                    if (iterations.Count > 2)
                    {
                        Console.WriteLine($"  Best:    Distance = {best.distance:F2} (at iteration {best.iteration})");
                    }
                    Console.WriteLine($"  Final:   Distance = {last.distance:F2}");
                    Console.WriteLine($"  Total iterations: {iterations.Count}");
                }

                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"\n✓ Final Solution:");
                Console.WriteLine($"  Distance: {tour.TotalDistance:F2}");
                Console.WriteLine($"  Route: {string.Join(" → ", tour.Cities.Select(c => c.Name))} → {tour.Cities[0].Name}");

                DrawSimpleVisualization(tour);

                if (algorithms.Last() != (name, solver))
                {
                    Console.WriteLine("\nPress any key for next algorithm...");
                    Console.ReadKey();
                    Console.WriteLine(); // Add newline after keypress
                }
            }

            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine("Demonstration Complete! All algorithms have been demonstrated.");
        }

        static void ShowAlgorithmInfo()
        {
            PrintSectionHeader("Algorithm Information");

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
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n📍 {algo}");
                Console.ResetColor();
                Console.WriteLine(new string('-', 40));
                Console.WriteLine($"Description: {description}");
                Console.WriteLine($"Complexity:  {complexity}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Pros:        {pros}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Cons:        {cons}");
                Console.ResetColor();
            }

            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine("\n💡 Recommendations:");
            Console.WriteLine("  • Small problems (< 20 cities): Nearest Neighbor + 2-Opt");
            Console.WriteLine("  • Medium problems (20-100 cities): Simulated Annealing");
            Console.WriteLine("  • Large problems (> 100 cities): Genetic Algorithm");
            Console.WriteLine("  • Real-time requirements: Nearest Neighbor");
            Console.WriteLine("  • Best quality: Genetic Algorithm with tuned parameters");
        }

        static void DrawSimpleVisualization(Tour tour)
        {
            Console.WriteLine("\nSimple ASCII Visualization:");
            Console.WriteLine(new string('─', 50));

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
                Console.Write("  ");
                for (int j = 0; j < width; j++)
                {
                    if (grid[i, j] == '●')
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(grid[i, j]);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(grid[i, j]);
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine(new string('─', 50));
        }

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

        static ConsoleColor GetColorForRatio(double ratio)
        {
            if (ratio <= 1.05) return ConsoleColor.Green;
            if (ratio <= 1.15) return ConsoleColor.Yellow;
            if (ratio <= 1.30) return ConsoleColor.DarkYellow;
            return ConsoleColor.Red;
        }

        // Removed ProgressBar class that was overwriting console lines
    }
}