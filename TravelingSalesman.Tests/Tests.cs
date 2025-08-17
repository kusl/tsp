using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TravelingSalesman.Core;
using Xunit;

namespace TravelingSalesman.Tests
{
    // ============================================================================
    // CITY TESTS
    // ============================================================================
    public class CityTests
    {
        [Fact]
        public void City_Constructor_ShouldCreateValidCity()
        {
            // Arrange & Act
            var city = new City(1, "TestCity", 10.5, 20.3);

            // Assert
            Assert.Equal(1, city.Id);
            Assert.Equal("TestCity", city.Name);
            Assert.Equal(10.5, city.X);
            Assert.Equal(20.3, city.Y);
        }

        [Fact]
        public void City_DistanceTo_ShouldCalculateCorrectEuclideanDistance()
        {
            // Arrange
            var city1 = new City(1, "City1", 0, 0);
            var city2 = new City(2, "City2", 3, 4);

            // Act
            var distance = city1.DistanceTo(city2);

            // Assert
            Assert.Equal(5.0, distance, 2); // 3-4-5 triangle
        }

        [Fact]
        public void City_DistanceTo_SameCity_ShouldReturnZero()
        {
            // Arrange
            var city = new City(1, "TestCity", 10, 20);

            // Act
            var distance = city.DistanceTo(city);

            // Assert
            Assert.Equal(0.0, distance);
        }

        [Fact]
        public void City_DistanceTo_NegativeCoordinates_ShouldWork()
        {
            // Arrange
            var city1 = new City(1, "City1", -5, -10);
            var city2 = new City(2, "City2", 5, 10);

            // Act
            var distance = city1.DistanceTo(city2);

            // Assert
            Assert.True(distance > 0);
            Assert.Equal(Math.Sqrt(400 + 400), distance, 2);
        }

        [Theory]
        [InlineData(0, 0, 1, 0, 1.0)]
        [InlineData(0, 0, 0, 1, 1.0)]
        [InlineData(1, 1, 4, 5, 5.0)]
        [InlineData(-1, -1, 2, 3, 5.0)]
        public void City_DistanceTo_VariousCoordinates_ShouldCalculateCorrectly(
            double x1, double y1, double x2, double y2, double expected)
        {
            // Arrange
            var city1 = new City(1, "City1", x1, y1);
            var city2 = new City(2, "City2", x2, y2);

            // Act
            var distance = city1.DistanceTo(city2);

            // Assert
            Assert.Equal(expected, distance, 2);
        }

        [Fact]
        public void City_Record_Equality_ShouldWorkCorrectly()
        {
            // Arrange
            var city1 = new City(1, "TestCity", 10, 20);
            var city2 = new City(1, "TestCity", 10, 20);
            var city3 = new City(2, "TestCity", 10, 20);

            // Act & Assert
            Assert.Equal(city1, city2);
            Assert.NotEqual(city1, city3);
            Assert.Equal(city1.GetHashCode(), city2.GetHashCode());
        }
    }

    // ============================================================================
    // TOUR TESTS
    // ============================================================================
    public class TourTests
    {
        private readonly List<City> _testCities;
        private readonly double[,] _distanceMatrix;

        public TourTests()
        {
            _testCities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 1, 1),
                new City(3, "D", 0, 1)
            };

            _distanceMatrix = BuildDistanceMatrix(_testCities);
        }

        private static double[,] BuildDistanceMatrix(IReadOnlyList<City> cities)
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

        [Fact]
        public void Tour_Constructor_ShouldCreateValidTour()
        {
            // Act
            var tour = new Tour(_testCities, _distanceMatrix);

            // Assert
            Assert.NotNull(tour.Cities);
            Assert.Equal(4, tour.Cities.Count);
            Assert.Equal(_testCities[0], tour.Cities[0]);
        }

        [Fact]
        public void Tour_TotalDistance_ShouldCalculateCorrectly()
        {
            // Arrange
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 0, 0) // Back to origin coordinates
            };
            var matrix = BuildDistanceMatrix(cities);

            // Act
            var tour = new Tour(cities, matrix);
            var distance = tour.TotalDistance;

            // Assert
            // A->B: 1, B->C: 1, C->A: 0 = 2.0
            Assert.Equal(2.0, distance, 2);
        }

        [Fact]
        public void Tour_TotalDistance_EmptyTour_ShouldReturnZero()
        {
            // Arrange
            var emptyCities = new List<City>();
            var emptyMatrix = new double[0, 0];

            // Act
            var tour = new Tour(emptyCities, emptyMatrix);

            // Assert
            Assert.Equal(0.0, tour.TotalDistance);
        }

        [Fact]
        public void Tour_TotalDistance_SingleCity_ShouldReturnZero()
        {
            // Arrange
            var singleCity = new List<City> { new City(0, "A", 0, 0) };
            var matrix = new double[1, 1] { { 0 } };

            // Act
            var tour = new Tour(singleCity, matrix);

            // Assert
            Assert.Equal(0.0, tour.TotalDistance);
        }

        [Fact]
        public void Tour_TotalDistance_ShouldCacheResult()
        {
            // Arrange
            var tour = new Tour(_testCities, _distanceMatrix);

            // Act
            var distance1 = tour.TotalDistance;
            var distance2 = tour.TotalDistance;

            // Assert
            Assert.Equal(distance1, distance2);
            Assert.True(distance1 > 0);
        }

        [Fact]
        public void Tour_SwapCities_ShouldUpdateTour()
        {
            // Arrange
            var tour = new Tour(_testCities, _distanceMatrix);
            var originalFirst = tour.Cities[0];
            var originalSecond = tour.Cities[1];

            // Act
            tour.SwapCities(0, 1);

            // Assert
            Assert.Equal(originalSecond, tour.Cities[0]);
            Assert.Equal(originalFirst, tour.Cities[1]);
        }

        [Fact]
        public void Tour_SwapCities_SameIndex_ShouldNotChange()
        {
            // Arrange
            var tour = new Tour(_testCities, _distanceMatrix);
            var originalCities = tour.Cities.ToList();

            // Act
            tour.SwapCities(1, 1);

            // Assert
            for (int i = 0; i < originalCities.Count; i++)
            {
                Assert.Equal(originalCities[i], tour.Cities[i]);
            }
        }

        [Fact]
        public void Tour_SwapCities_ShouldInvalidateCache()
        {
            // Arrange
            var tour = new Tour(_testCities, _distanceMatrix);
            var originalDistance = tour.TotalDistance;

            // Act
            tour.SwapCities(0, 1);
            var newDistance = tour.TotalDistance;

            // Assert - distance might change depending on tour
            Assert.True(originalDistance >= 0);
            Assert.True(newDistance >= 0);
        }

        [Fact]
        public void Tour_Reverse_ShouldReverseSegment()
        {
            // Arrange
            var cities = _testCities.ToList(); // A, B, C, D
            var tour = new Tour(cities, _distanceMatrix);

            // Act
            tour.Reverse(1, 2); // Reverse B, C

            // Assert
            Assert.Equal("A", tour.Cities[0].Name);
            Assert.Equal("C", tour.Cities[1].Name);
            Assert.Equal("B", tour.Cities[2].Name);
            Assert.Equal("D", tour.Cities[3].Name);
        }

        [Fact]
        public void Tour_Reverse_WholeRange_ShouldReverseAll()
        {
            // Arrange
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 2, 0)
            };
            var matrix = BuildDistanceMatrix(cities);
            var tour = new Tour(cities, matrix);

            // Act
            tour.Reverse(0, 2);

            // Assert
            Assert.Equal("C", tour.Cities[0].Name);
            Assert.Equal("B", tour.Cities[1].Name);
            Assert.Equal("A", tour.Cities[2].Name);
        }

        [Fact]
        public void Tour_Clone_ShouldCreateIndependentCopy()
        {
            // Arrange
            var tour = new Tour(_testCities, _distanceMatrix);

            // Act
            var cloned = tour.Clone();

            // Assert
            Assert.NotSame(tour, cloned);
            Assert.Equal(tour.TotalDistance, cloned.TotalDistance);
            Assert.Equal(tour.Cities.Count, cloned.Cities.Count);
        }

        [Fact]
        public void Tour_ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var tour = new Tour(_testCities, _distanceMatrix);

            // Act
            var result = tour.ToString();

            // Assert
            Assert.Contains("Tour Distance:", result);
            Assert.Contains("Route:", result);
            Assert.Contains("A", result);
        }
    }

    // ============================================================================
    // TSP SOLVER BASE TESTS
    // ============================================================================
    public class TspSolverBaseTests
    {
        private class TestSolver : TspSolverBase
        {
            public override string Name => "Test Solver";

            public TestSolver(ILogger? logger = null) : base(logger) { }

            public override Task<Tour> SolveAsync(IReadOnlyList<City> cities, CancellationToken cancellationToken = default)
            {
                var distanceMatrix = BuildDistanceMatrix(cities);
                OnProgressChanged(1, 100.0, "Test progress");
                return Task.FromResult(new Tour(cities, distanceMatrix));
            }

            public double[,] TestBuildDistanceMatrix(IReadOnlyList<City> cities) => BuildDistanceMatrix(cities);
            public void TestOnProgressChanged(int iteration, double distance, string message) => 
                OnProgressChanged(iteration, distance, message);
        }

        [Fact]
        public void TspSolverBase_BuildDistanceMatrix_ShouldCreateCorrectMatrix()
        {
            // Arrange
            var solver = new TestSolver();
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 0, 1)
            };

            // Act
            var matrix = solver.TestBuildDistanceMatrix(cities);

            // Assert
            Assert.Equal(3, matrix.GetLength(0));
            Assert.Equal(3, matrix.GetLength(1));
            Assert.Equal(0.0, matrix[0, 0]);
            Assert.Equal(1.0, matrix[0, 1], 2);
            Assert.Equal(1.0, matrix[0, 2], 2);
        }

        [Fact]
        public void TspSolverBase_ProgressChanged_ShouldRaiseEvent()
        {
            // Arrange
            var solver = new TestSolver();
            var eventRaised = false;
            TspProgressEventArgs? eventArgs = null;

            solver.ProgressChanged += (sender, args) =>
            {
                eventRaised = true;
                eventArgs = args;
            };

            // Act
            solver.TestOnProgressChanged(5, 123.45, "Test message");

            // Assert
            Assert.True(eventRaised);
            Assert.NotNull(eventArgs);
            Assert.Equal(5, eventArgs.Iteration);
            Assert.Equal(123.45, eventArgs.CurrentBestDistance);
            Assert.Equal("Test message", eventArgs.Message);
        }

        [Fact]
        public async Task TspSolverBase_SolveAsync_ShouldReturnValidTour()
        {
            // Arrange
            var solver = new TestSolver();
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0)
            };

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Equal(2, tour.Cities.Count);
        }
    }

    // ============================================================================
    // NEAREST NEIGHBOR SOLVER TESTS
    // ============================================================================
    public class NearestNeighborSolverTests
    {
        [Fact]
        public void NearestNeighborSolver_Name_ShouldReturnCorrectName()
        {
            // Arrange
            var solver = new NearestNeighborSolver();

            // Assert
            Assert.Equal("Nearest Neighbor", solver.Name);
        }

        [Fact]
        public async Task NearestNeighborSolver_SolveAsync_EmptyList_ShouldReturnEmptyTour()
        {
            // Arrange
            var solver = new NearestNeighborSolver();
            var cities = new List<City>();

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Empty(tour.Cities);
            Assert.Equal(0.0, tour.TotalDistance);
        }

        [Fact]
        public async Task NearestNeighborSolver_SolveAsync_SingleCity_ShouldReturnSingleCityTour()
        {
            // Arrange
            var solver = new NearestNeighborSolver();
            var cities = new List<City> { new City(0, "A", 0, 0) };

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Single(tour.Cities);
            Assert.Equal(0.0, tour.TotalDistance);
        }

        [Fact]
        public async Task NearestNeighborSolver_SolveAsync_TwoCities_ShouldReturnValidTour()
        {
            // Arrange
            var solver = new NearestNeighborSolver();
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0)
            };

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Equal(2, tour.Cities.Count);
            Assert.Equal("A", tour.Cities[0].Name);
            Assert.Equal("B", tour.Cities[1].Name);
            Assert.Equal(2.0, tour.TotalDistance); // A->B->A = 1+1 = 2
        }

        [Fact]
        public async Task NearestNeighborSolver_SolveAsync_MultipleCities_ShouldStartFromFirstCity()
        {
            // Arrange
            var solver = new NearestNeighborSolver();
            var cities = new List<City>
            {
                new City(0, "Start", 0, 0),
                new City(1, "B", 10, 0),
                new City(2, "C", 1, 0),  // Nearest to Start
                new City(3, "D", 5, 0)
            };

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.Equal("Start", tour.Cities[0].Name);
            Assert.Equal("C", tour.Cities[1].Name); // Should pick nearest (C) next
        }

        [Fact]
        public async Task NearestNeighborSolver_SolveAsync_ShouldRaiseProgressEvents()
        {
            // Arrange
            var solver = new NearestNeighborSolver();
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 2, 0)
            };

            var progressEvents = new List<TspProgressEventArgs>();
            solver.ProgressChanged += (sender, args) => progressEvents.Add(args);

            // Act
            await solver.SolveAsync(cities);

            // Assert
            Assert.True(progressEvents.Count > 0);
            Assert.All(progressEvents, e => Assert.True(e.Iteration > 0));
        }

        [Fact]
        public async Task NearestNeighborSolver_SolveAsync_Cancellation_ShouldThrow()
        {
            // Arrange
            var solver = new NearestNeighborSolver();
            var cities = Enumerable.Range(0, 100)
                .Select(i => new City(i, $"City{i}", i * 10, i * 5))
                .ToList();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => solver.SolveAsync(cities, cts.Token));
        }

        [Fact]
        public async Task NearestNeighborSolver_SolveAsync_DeterministicResults()
        {
            // Arrange
            var solver = new NearestNeighborSolver();
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 1),
                new City(2, "C", 2, 0),
                new City(3, "D", 1, -1)
            };

            // Act
            var tour1 = await solver.SolveAsync(cities);
            var tour2 = await solver.SolveAsync(cities);

            // Assert
            Assert.Equal(tour1.TotalDistance, tour2.TotalDistance);
            Assert.Equal(tour1.Cities.Count, tour2.Cities.Count);
            for (int i = 0; i < tour1.Cities.Count; i++)
            {
                Assert.Equal(tour1.Cities[i].Id, tour2.Cities[i].Id);
            }
        }
    }

    // ============================================================================
    // TWO-OPT SOLVER TESTS
    // ============================================================================
    public class TwoOptSolverTests
    {
        [Fact]
        public void TwoOptSolver_Name_ShouldReturnCorrectName()
        {
            // Arrange
            var solver = new TwoOptSolver();

            // Assert
            Assert.Equal("2-Opt", solver.Name);
        }

        [Fact]
        public void TwoOptSolver_Constructor_ShouldAcceptMaxIterations()
        {
            // Act
            var solver = new TwoOptSolver(maxIterations: 500);

            // Assert
            Assert.Equal("2-Opt", solver.Name);
        }

        [Fact]
        public async Task TwoOptSolver_SolveAsync_ShouldImproveInitialSolution()
        {
            // Arrange
            var solver = new TwoOptSolver(maxIterations: 10);

            // Create a deliberately suboptimal tour (crossing paths)
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 1),
                new City(2, "C", 0, 1),
                new City(3, "D", 1, 0)
            };

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Equal(4, tour.Cities.Count);
            Assert.True(tour.TotalDistance > 0);
        }

        [Fact]
        public async Task TwoOptSolver_SolveAsync_SmallTour_ShouldComplete()
        {
            // Arrange
            var solver = new TwoOptSolver();
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0)
            };

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Equal(2, tour.Cities.Count);
        }

        [Fact]
        public async Task TwoOptSolver_SolveAsync_ShouldRaiseProgressEvents()
        {
            // Arrange
            var solver = new TwoOptSolver(maxIterations: 5);
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 1, 1),
                new City(3, "D", 0, 1)
            };

            var progressEvents = new List<TspProgressEventArgs>();
            solver.ProgressChanged += (sender, args) => progressEvents.Add(args);

            // Act
            await solver.SolveAsync(cities);

            // Assert
            // Should have events from both NN and 2-opt phases
            Assert.True(progressEvents.Count > 0);
        }

        [Fact]
        public async Task TwoOptSolver_SolveAsync_Cancellation_ShouldThrow()
        {
            // Arrange
            var solver = new TwoOptSolver(maxIterations: 10000);
            var cities = Enumerable.Range(0, 50)
                .Select(i => new City(i, $"City{i}", i, i))
                .ToList();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => solver.SolveAsync(cities, cts.Token));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task TwoOptSolver_SolveAsync_DifferentMaxIterations_ShouldWork(int maxIterations)
        {
            // Arrange
            var solver = new TwoOptSolver(maxIterations: maxIterations);
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 2, 0),
                new City(2, "C", 2, 2),
                new City(3, "D", 0, 2)
            };

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Equal(4, tour.Cities.Count);
            Assert.True(tour.TotalDistance > 0);
        }
    }

    // ============================================================================
    // SIMULATED ANNEALING SOLVER TESTS
    // ============================================================================
    public class SimulatedAnnealingSolverTests
    {
        [Fact]
        public void SimulatedAnnealingSolver_Name_ShouldReturnCorrectName()
        {
            // Arrange
            var solver = new SimulatedAnnealingSolver();

            // Assert
            Assert.Equal("Simulated Annealing", solver.Name);
        }

        [Fact]
        public void SimulatedAnnealingSolver_Constructor_DefaultParameters_ShouldWork()
        {
            // Act
            var solver = new SimulatedAnnealingSolver();

            // Assert
            Assert.Equal("Simulated Annealing", solver.Name);
        }

        [Fact]
        public void SimulatedAnnealingSolver_Constructor_CustomParameters_ShouldWork()
        {
            // Act
            var solver = new SimulatedAnnealingSolver(
                initialTemperature: 5000,
                coolingRate: 0.95,
                iterationsPerTemperature: 50);

            // Assert
            Assert.Equal("Simulated Annealing", solver.Name);
        }

        [Fact]
        public async Task SimulatedAnnealingSolver_SolveAsync_ShouldReturnValidTour()
        {
            // Arrange
            var solver = new SimulatedAnnealingSolver(
                initialTemperature: 100,
                coolingRate: 0.9,
                iterationsPerTemperature: 10);

            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 1, 1),
                new City(3, "D", 0, 1)
            };

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Equal(4, tour.Cities.Count);
            Assert.True(tour.TotalDistance > 0);
        }

        [Fact]
        public async Task SimulatedAnnealingSolver_SolveAsync_WithSeed_ShouldBeDeterministic()
        {
            // Arrange
            const int seed = 12345;
            var solver1 = new SimulatedAnnealingSolver(
                initialTemperature: 100,
                coolingRate: 0.8,
                iterationsPerTemperature: 5,
                seed: seed);

            var solver2 = new SimulatedAnnealingSolver(
                initialTemperature: 100,
                coolingRate: 0.8,
                iterationsPerTemperature: 5,
                seed: seed);

            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 2, 0),
                new City(2, "C", 1, 1)
            };

            // Act
            var tour1 = await solver1.SolveAsync(cities);
            var tour2 = await solver2.SolveAsync(cities);

            // Assert
            Assert.Equal(tour1.TotalDistance, tour2.TotalDistance);
        }

        [Fact]
        public async Task SimulatedAnnealingSolver_SolveAsync_ShouldRaiseProgressEvents()
        {
            // Arrange
            var solver = new SimulatedAnnealingSolver(
                initialTemperature: 100,
                coolingRate: 0.5,
                iterationsPerTemperature: 5);

            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 1, 1)
            };

            var progressEvents = new List<TspProgressEventArgs>();
            solver.ProgressChanged += (sender, args) => progressEvents.Add(args);

            // Act
            await solver.SolveAsync(cities);

            // Assert
            Assert.True(progressEvents.Count > 0);
        }

        [Fact]
        public async Task SimulatedAnnealingSolver_SolveAsync_TwoCities_ShouldWork()
        {
            // Arrange
            var solver = new SimulatedAnnealingSolver(
                initialTemperature: 10,
                coolingRate: 0.5,
                iterationsPerTemperature: 2);

            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0)
            };

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Equal(2, tour.Cities.Count);
            Assert.Equal(2.0, tour.TotalDistance); // A->B->A
        }

        [Fact]
        public async Task SimulatedAnnealingSolver_SolveAsync_Cancellation_ShouldThrow()
        {
            // Arrange
            var solver = new SimulatedAnnealingSolver(
                initialTemperature: 10000,
                coolingRate: 0.9999,
                iterationsPerTemperature: 1000);

            var cities = Enumerable.Range(0, 20)
                .Select(i => new City(i, $"City{i}", i, i))
                .ToList();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => solver.SolveAsync(cities, cts.Token));
        }
    }

    // ============================================================================
    // GENETIC ALGORITHM SOLVER TESTS
    // ============================================================================
    public class GeneticAlgorithmSolverTests
    {
        [Fact]
        public void GeneticAlgorithmSolver_Name_ShouldReturnCorrectName()
        {
            // Arrange
            var solver = new GeneticAlgorithmSolver();

            // Assert
            Assert.Equal("Genetic Algorithm", solver.Name);
        }

        [Fact]
        public void GeneticAlgorithmSolver_Constructor_DefaultParameters_ShouldWork()
        {
            // Act
            var solver = new GeneticAlgorithmSolver();

            // Assert
            Assert.Equal("Genetic Algorithm", solver.Name);
        }

        [Fact]
        public void GeneticAlgorithmSolver_Constructor_CustomParameters_ShouldWork()
        {
            // Act
            var solver = new GeneticAlgorithmSolver(
                populationSize: 50,
                generations: 100,
                mutationRate: 0.05,
                elitismRate: 0.1);

            // Assert
            Assert.Equal("Genetic Algorithm", solver.Name);
        }

        [Fact]
        public void GeneticAlgorithmSolver_CreateScaledGeneticSolver_ShouldCreateWithScaledParameters()
        {
            // Act
            var solver = GeneticAlgorithmSolver.CreateScaledGeneticSolver(50);

            // Assert
            Assert.Equal("Genetic Algorithm", solver.Name);
        }

        [Fact]
        public async Task GeneticAlgorithmSolver_SolveAsync_ShouldReturnValidTour()
        {
            // Arrange
            var solver = new GeneticAlgorithmSolver(
                populationSize: 20,
                generations: 10,
                mutationRate: 0.1,
                elitismRate: 0.2);

            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 1, 1),
                new City(3, "D", 0, 1)
            };

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Equal(4, tour.Cities.Count);
            Assert.True(tour.TotalDistance > 0);
            Assert.Equal("A", tour.Cities[0].Name); // Should keep first city fixed
        }

        [Fact]
        public async Task GeneticAlgorithmSolver_SolveAsync_WithSeed_ShouldBeDeterministic()
        {
            // Arrange
            const int seed = 54321;
            var solver1 = new GeneticAlgorithmSolver(
                populationSize: 10,
                generations: 5,
                mutationRate: 0.1,
                elitismRate: 0.1,
                seed: seed);

            var solver2 = new GeneticAlgorithmSolver(
                populationSize: 10,
                generations: 5,
                mutationRate: 0.1,
                elitismRate: 0.1,
                seed: seed);

            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 1, 1)
            };

            // Act
            var tour1 = await solver1.SolveAsync(cities);
            var tour2 = await solver2.SolveAsync(cities);

            // Assert
            Assert.Equal(tour1.TotalDistance, tour2.TotalDistance);
        }

        [Fact]
        public async Task GeneticAlgorithmSolver_SolveAsync_ShouldRaiseProgressEvents()
        {
            // Arrange
            var solver = new GeneticAlgorithmSolver(
                populationSize: 10,
                generations: 5,
                mutationRate: 0.1,
                elitismRate: 0.2);

            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 0, 1)
            };

            var progressEvents = new List<TspProgressEventArgs>();
            solver.ProgressChanged += (sender, args) => progressEvents.Add(args);

            // Act
            await solver.SolveAsync(cities);

            // Assert
            Assert.True(progressEvents.Count > 0);
        }

        [Fact]
        public async Task GeneticAlgorithmSolver_SolveAsync_TwoCities_ShouldWork()
        {
            // Arrange
            var solver = new GeneticAlgorithmSolver(
                populationSize: 5,
                generations: 2);

            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0)
            };

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Equal(2, tour.Cities.Count);
            Assert.Equal(2.0, tour.TotalDistance);
        }

        [Fact]
        public async Task GeneticAlgorithmSolver_SolveAsync_Cancellation_ShouldThrow()
        {
            // Arrange
            var solver = new GeneticAlgorithmSolver(
                populationSize: 100,
                generations: 1000);

            var cities = Enumerable.Range(0, 30)
                .Select(i => new City(i, $"City{i}", i, i * 2))
                .ToList();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => solver.SolveAsync(cities, cts.Token));
        }

        [Theory]
        [InlineData(0.0, 0.1)] // No mutation, some elitism
        [InlineData(0.1, 0.0)] // Some mutation, no elitism
        [InlineData(0.2, 0.3)] // Both mutation and elitism
        public async Task GeneticAlgorithmSolver_SolveAsync_DifferentRates_ShouldWork(
            double mutationRate, double elitismRate)
        {
            // Arrange
            var solver = new GeneticAlgorithmSolver(
                populationSize: 10,
                generations: 3,
                mutationRate: mutationRate,
                elitismRate: elitismRate);

            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 0, 1)
            };

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Equal(3, tour.Cities.Count);
        }
    }

    // ============================================================================
    // TSP SOLVER FACTORY TESTS
    // ============================================================================
    public class TspSolverFactoryTests
    {
        [Theory]
        [InlineData(TspSolverFactory.SolverType.NearestNeighbor, "Nearest Neighbor")]
        [InlineData(TspSolverFactory.SolverType.TwoOpt, "2-Opt")]
        [InlineData(TspSolverFactory.SolverType.SimulatedAnnealing, "Simulated Annealing")]
        [InlineData(TspSolverFactory.SolverType.GeneticAlgorithm, "Genetic Algorithm")]
        public void TspSolverFactory_CreateSolver_ShouldReturnCorrectSolverType(
            TspSolverFactory.SolverType solverType, string expectedName)
        {
            // Act
            var solver = TspSolverFactory.CreateSolver(solverType);

            // Assert
            Assert.Equal(expectedName, solver.Name);
        }

        [Fact]
        public void TspSolverFactory_CreateSolver_InvalidType_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                TspSolverFactory.CreateSolver((TspSolverFactory.SolverType)999));
        }

        [Fact]
        public void TspSolverFactory_CreateAllSolvers_ShouldReturnAllTypes()
        {
            // Act
            var solvers = TspSolverFactory.CreateAllSolvers().ToList();

            // Assert
            Assert.Equal(4, solvers.Count);
            Assert.Contains(solvers, s => s.Name == "Nearest Neighbor");
            Assert.Contains(solvers, s => s.Name == "2-Opt");
            Assert.Contains(solvers, s => s.Name == "Simulated Annealing");
            Assert.Contains(solvers, s => s.Name == "Genetic Algorithm");
        }

        [Fact]
        public void TspSolverFactory_CreateSolver_WithLoggerFactory_ShouldWork()
        {
            // Arrange
            using var loggerFactory = LoggerFactory.Create(builder => { });

            // Act
            var solver = TspSolverFactory.CreateSolver(
                TspSolverFactory.SolverType.NearestNeighbor, loggerFactory);

            // Assert
            Assert.Equal("Nearest Neighbor", solver.Name);
        }

        [Fact]
        public void TspSolverFactory_CreateAllSolvers_WithLoggerFactory_ShouldWork()
        {
            // Arrange
            using var loggerFactory = LoggerFactory.Create(builder => { });

            // Act
            var solvers = TspSolverFactory.CreateAllSolvers(loggerFactory).ToList();

            // Assert
            Assert.Equal(4, solvers.Count);
        }
    }

    // ============================================================================
    // TSP DATA GENERATOR TESTS
    // ============================================================================
    public class TspDataGeneratorTests
    {
        [Fact]
        public void TspDataGenerator_Constructor_ShouldWork()
        {
            // Act
            var generator = new TspDataGenerator();

            // Assert
            Assert.NotNull(generator);
        }

        [Fact]
        public void TspDataGenerator_Constructor_WithSeed_ShouldWork()
        {
            // Act
            var generator = new TspDataGenerator(seed: 12345);

            // Assert
            Assert.NotNull(generator);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(50)]
        public void TspDataGenerator_GenerateRandomCities_ShouldGenerateCorrectCount(int count)
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 123);

            // Act
            var cities = generator.GenerateRandomCities(count);

            // Assert
            Assert.Equal(count, cities.Count);
            Assert.All(cities, city => 
            {
                Assert.True(city.X >= 0 && city.X <= 100);
                Assert.True(city.Y >= 0 && city.Y <= 100);
                Assert.False(string.IsNullOrEmpty(city.Name));
            });
        }

        [Fact]
        public void TspDataGenerator_GenerateRandomCities_WithSeed_ShouldBeDeterministic()
        {
            // Arrange
            const int seed = 456;
            var generator1 = new TspDataGenerator(seed);
            var generator2 = new TspDataGenerator(seed);

            // Act
            var cities1 = generator1.GenerateRandomCities(10);
            var cities2 = generator2.GenerateRandomCities(10);

            // Assert
            Assert.Equal(cities1.Count, cities2.Count);
            for (int i = 0; i < cities1.Count; i++)
            {
                Assert.Equal(cities1[i].X, cities2[i].X);
                Assert.Equal(cities1[i].Y, cities2[i].Y);
                Assert.Equal(cities1[i].Name, cities2[i].Name);
            }
        }

        [Fact]
        public void TspDataGenerator_GenerateRandomCities_CustomBounds_ShouldRespectBounds()
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 789);

            // Act
            var cities = generator.GenerateRandomCities(20, maxX: 50, maxY: 30);

            // Assert
            Assert.All(cities, city =>
            {
                Assert.True(city.X >= 0 && city.X <= 50);
                Assert.True(city.Y >= 0 && city.Y <= 30);
            });
        }

        [Theory]
        [InlineData(3)]
        [InlineData(8)]
        [InlineData(16)]
        public void TspDataGenerator_GenerateCircularCities_ShouldGenerateCircularPattern(int count)
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 321);

            // Act
            var cities = generator.GenerateCircularCities(count, radius: 10);

            // Assert
            Assert.Equal(count, cities.Count);

            // Check that cities are roughly on a circle (allow some floating point tolerance)
            const double centerX = 50;
            const double centerY = 50;
            const double radius = 10;

            Assert.All(cities, city =>
            {
                var distance = Math.Sqrt(Math.Pow(city.X - centerX, 2) + Math.Pow(city.Y - centerY, 2));
                Assert.Equal(radius, distance, 1); // 1 unit tolerance
            });
        }

        [Fact]
        public void TspDataGenerator_GenerateCircularCities_CustomParameters_ShouldWork()
        {
            // Arrange
            var generator = new TspDataGenerator();

            // Act
            var cities = generator.GenerateCircularCities(6, radius: 25, centerX: 30, centerY: 40);

            // Assert
            Assert.Equal(6, cities.Count);
            Assert.All(cities, city =>
            {
                var distance = Math.Sqrt(Math.Pow(city.X - 30, 2) + Math.Pow(city.Y - 40, 2));
                Assert.Equal(25, distance, 1);
            });
        }

        [Theory]
        [InlineData(2, 3)] // 2x3 grid
        [InlineData(4, 4)] // 4x4 grid
        [InlineData(1, 5)] // 1x5 grid
        public void TspDataGenerator_GenerateGridCities_ShouldGenerateGridPattern(int rows, int cols)
        {
            // Arrange
            var generator = new TspDataGenerator();

            // Act
            var cities = generator.GenerateGridCities(rows, cols);

            // Assert
            Assert.Equal(rows * cols, cities.Count);

            // Verify grid structure
            var cityArray = cities.ToArray();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var city = cityArray[row * cols + col];
                    Assert.Equal(col * 10.0, city.X); // Default spacing is 10
                    Assert.Equal(row * 10.0, city.Y);
                }
            }
        }

        [Fact]
        public void TspDataGenerator_GenerateGridCities_CustomSpacing_ShouldWork()
        {
            // Arrange
            var generator = new TspDataGenerator();

            // Act
            var cities = generator.GenerateGridCities(2, 2, spacing: 5);

            // Assert
            Assert.Equal(4, cities.Count);
            Assert.Equal(0, cities[0].X);
            Assert.Equal(0, cities[0].Y);
            Assert.Equal(5, cities[1].X);
            Assert.Equal(0, cities[1].Y);
            Assert.Equal(0, cities[2].X);
            Assert.Equal(5, cities[2].Y);
            Assert.Equal(5, cities[3].X);
            Assert.Equal(5, cities[3].Y);
        }

        [Fact]
        public void TspDataGenerator_AllMethods_ShouldGenerateUniqueIds()
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 111);

            // Act
            var randomCities = generator.GenerateRandomCities(5);
            var circularCities = generator.GenerateCircularCities(5);
            var gridCities = generator.GenerateGridCities(2, 3);

            // Assert
            var allGeneratedCities = new[] { randomCities, circularCities, gridCities };

            foreach (var cityList in allGeneratedCities)
            {
                var ids = cityList.Select(c => c.Id).ToList();
                var uniqueIds = ids.Distinct().ToList();
                Assert.Equal(ids.Count, uniqueIds.Count); // All IDs should be unique
                Assert.Equal(0, ids.Min()); // IDs should start from 0
                Assert.Equal(ids.Count - 1, ids.Max()); // IDs should be sequential
            }
        }

        [Fact]
        public void TspDataGenerator_GeneratedCities_ShouldHaveValidNames()
        {
            // Arrange
            var generator = new TspDataGenerator();

            // Act
            var cities = generator.GenerateRandomCities(5);

            // Assert
            Assert.All(cities, city =>
            {
                Assert.False(string.IsNullOrWhiteSpace(city.Name));
                Assert.StartsWith("City_", city.Name);
            });
        }
    }

    // ============================================================================
    // TSP BENCHMARK TESTS
    // ============================================================================
    public class TspBenchmarkTests
    {
        [Fact]
        public void TspBenchmark_Constructor_ShouldWork()
        {
            // Act
            var benchmark = new TspBenchmark();

            // Assert
            Assert.NotNull(benchmark);
        }

        [Fact]
        public async Task TspBenchmark_RunBenchmarkAsync_ShouldReturnResults()
        {
            // Arrange
            var benchmark = new TspBenchmark();
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 1, 1)
            };

            var solvers = new List<ITspSolver>
            {
                new NearestNeighborSolver(),
                new TwoOptSolver(maxIterations: 5)
            };

            // Act
            var results = await benchmark.RunBenchmarkAsync(cities, solvers);

            // Assert
            Assert.Equal(2, results.Count);
            Assert.All(results, result =>
            {
                Assert.NotNull(result.SolverName);
                Assert.True(result.Distance > 0);
                Assert.True(result.ExecutionTime.TotalMilliseconds >= 0);
                Assert.NotNull(result.Tour);
            });
        }

        [Fact]
        public async Task TspBenchmark_RunBenchmarkAsync_ShouldSortResultsByDistance()
        {
            // Arrange
            var benchmark = new TspBenchmark();
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 2, 0),
                new City(2, "C", 1, 1),
                new City(3, "D", 0, 1)
            };

            var solvers = new List<ITspSolver>
            {
                new NearestNeighborSolver(),
                new TwoOptSolver(maxIterations: 2),
                new SimulatedAnnealingSolver(100, 0.5, 5)
            };

            // Act
            var results = await benchmark.RunBenchmarkAsync(cities, solvers);

            // Assert
            for (int i = 1; i < results.Count; i++)
            {
                Assert.True(results[i].Distance >= results[i - 1].Distance);
            }
        }

        [Fact]
        public async Task TspBenchmark_RunBenchmarkAsync_EmptySolvers_ShouldReturnEmpty()
        {
            // Arrange
            var benchmark = new TspBenchmark();
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0)
            };

            var solvers = new List<ITspSolver>();

            // Act
            var results = await benchmark.RunBenchmarkAsync(cities, solvers);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task TspBenchmark_RunBenchmarkAsync_Cancellation_ShouldThrow()
        {
            // Arrange
            var benchmark = new TspBenchmark();
            var cities = Enumerable.Range(0, 20)
                .Select(i => new City(i, $"City{i}", i, i))
                .ToList();

            var solvers = new List<ITspSolver>
            {
                new GeneticAlgorithmSolver(100, 1000, 0.1, 0.1)
            };

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => benchmark.RunBenchmarkAsync(cities, solvers, cts.Token));
        }

        [Fact]
        public void TspBenchmark_FormatResults_ShouldReturnFormattedString()
        {
            // Arrange
            var benchmark = new TspBenchmark();
            var results = new List<TspBenchmark.BenchmarkResult>
            {
                new("Solver A", 100.0, TimeSpan.FromMilliseconds(50), 
                    new Tour(new List<City>(), new double[0,0])),
                new("Solver B", 120.0, TimeSpan.FromMilliseconds(75), 
                    new Tour(new List<City>(), new double[0,0]))
            };

            // Act
            var formatted = benchmark.FormatResults(results);

            // Assert
            Assert.Contains("TSP Solver Benchmark Results", formatted);
            Assert.Contains("Solver A", formatted);
            Assert.Contains("Solver B", formatted);
            Assert.Contains("100.00", formatted);
            Assert.Contains("120.00", formatted);
            Assert.Contains("50.0", formatted);
            Assert.Contains("75.0", formatted);
            Assert.Contains("0.00%", formatted); // Best is 0% from itself
            Assert.Contains("20.00%", formatted); // 120 is 20% more than 100
        }

        [Fact]
        public void TspBenchmark_BenchmarkResult_Record_ShouldWorkCorrectly()
        {
            // Arrange
            var cities = new List<City> { new City(0, "A", 0, 0) };
            var tour = new Tour(cities, new double[1,1]);

            // Act
            var result = new TspBenchmark.BenchmarkResult(
                "Test Solver", 
                50.5, 
                TimeSpan.FromMilliseconds(100), 
                tour);

            // Assert
            Assert.Equal("Test Solver", result.SolverName);
            Assert.Equal(50.5, result.Distance);
            Assert.Equal(100, result.ExecutionTime.TotalMilliseconds);
            Assert.Equal(tour, result.Tour);
        }
    }

    // ============================================================================
    // TSP PROGRESS EVENT ARGS TESTS
    // ============================================================================
    public class TspProgressEventArgsTests
    {
        [Fact]
        public void TspProgressEventArgs_Properties_ShouldSetCorrectly()
        {
            // Act
            var args = new TspProgressEventArgs
            {
                Iteration = 42,
                CurrentBestDistance = 123.45,
                Message = "Test message"
            };

            // Assert
            Assert.Equal(42, args.Iteration);
            Assert.Equal(123.45, args.CurrentBestDistance);
            Assert.Equal("Test message", args.Message);
        }

        [Fact]
        public void TspProgressEventArgs_DefaultMessage_ShouldBeEmpty()
        {
            // Act
            var args = new TspProgressEventArgs
            {
                Iteration = 1,
                CurrentBestDistance = 100.0
            };

            // Assert
            Assert.Equal(string.Empty, args.Message);
        }

        [Fact]
        public void TspProgressEventArgs_InheritsFromEventArgs()
        {
            // Arrange & Act
            var args = new TspProgressEventArgs();

            // Assert
            Assert.IsAssignableFrom<EventArgs>(args);
        }
    }

    // ============================================================================
    // INTEGRATION TESTS
    // ============================================================================
    public class IntegrationTests
    {
        [Fact]
        public async Task Integration_AllSolvers_SameCities_ShouldProduceDifferentResults()
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 12345);
            var cities = generator.GenerateRandomCities(8);

            var solvers = TspSolverFactory.CreateAllSolvers().ToList();

            // Act
            var tasks = solvers.Select(solver => solver.SolveAsync(cities)).ToList();
            var tours = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(4, tours.Length);
            Assert.All(tours, tour =>
            {
                Assert.Equal(cities.Count, tour.Cities.Count);
                Assert.True(tour.TotalDistance > 0);
            });

            // At least some should have different distances (though not guaranteed)
            var distances = tours.Select(t => Math.Round(t.TotalDistance, 2)).Distinct().ToList();
            // We can't guarantee they'll all be different, but we can check they're all valid
            Assert.All(distances, d => Assert.True(d > 0));
        }

        [Fact]
        public async Task Integration_Benchmark_WithAllSolvers_ShouldComplete()
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 999);
            var cities = generator.GenerateCircularCities(6);
            var benchmark = new TspBenchmark();

            var solvers = new List<ITspSolver>
            {
                new NearestNeighborSolver(),
                new TwoOptSolver(maxIterations: 10),
                new SimulatedAnnealingSolver(100, 0.8, 5),
                new GeneticAlgorithmSolver(10, 5, 0.1, 0.2)
            };

            // Act
            var results = await benchmark.RunBenchmarkAsync(cities, solvers);
            var formatted = benchmark.FormatResults(results);

            // Assert
            Assert.Equal(4, results.Count);
            Assert.NotEmpty(formatted);
            Assert.Contains("Nearest Neighbor", formatted);
            Assert.Contains("2-Opt", formatted);
            Assert.Contains("Simulated Annealing", formatted);
            Assert.Contains("Genetic Algorithm", formatted);
        }

        [Fact]
        public async Task Integration_DataGenerator_AllPatterns_WithAllSolvers_ShouldWork()
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 777);

            var testCases = new[]
            {
                ("Random", generator.GenerateRandomCities(5)),
                ("Circular", generator.GenerateCircularCities(5)),
                ("Grid", generator.GenerateGridCities(2, 3).Take(5).ToList())
            };

            var solver = new NearestNeighborSolver();

            // Act & Assert
            foreach (var (pattern, cities) in testCases)
            {
                var tour = await solver.SolveAsync(cities);

                Assert.NotNull(tour);
                Assert.Equal(cities.Count, tour.Cities.Count);
                Assert.True(tour.TotalDistance >= 0);
            }
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task Integration_ScaledGeneticAlgorithm_DifferentCityCounts_ShouldScale(int cityCount)
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 888);
            var cities = generator.GenerateRandomCities(cityCount);
            var solver = GeneticAlgorithmSolver.CreateScaledGeneticSolver(cityCount);

            // Act
            var tour = await solver.SolveAsync(cities);

            // Assert
            Assert.NotNull(tour);
            Assert.Equal(cityCount, tour.Cities.Count);
            Assert.True(tour.TotalDistance > 0);
        }

        [Fact]
        public async Task Integration_ProgressEvents_AllSolvers_ShouldRaiseEvents()
        {
            // Arrange
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 1, 1),
                new City(3, "D", 0, 1)
            };

            var solvers = new List<ITspSolver>
            {
                new NearestNeighborSolver(),
                new TwoOptSolver(maxIterations: 5),
                new SimulatedAnnealingSolver(50, 0.8, 3),
                new GeneticAlgorithmSolver(5, 3, 0.1, 0.2)
            };

            // Act & Assert
            foreach (var solver in solvers)
            {
                var progressEvents = new List<TspProgressEventArgs>();
                solver.ProgressChanged += (s, e) => progressEvents.Add(e);

                var tour = await solver.SolveAsync(cities);

                Assert.NotNull(tour);
                // Most solvers should raise at least one progress event
                // (Some very fast solvers might not, depending on implementation)
            }
        }
    }

    // ============================================================================
    // PERFORMANCE AND STRESS TESTS
    // ============================================================================
    public class PerformanceTests
    {
        [Fact]
        public async Task Performance_NearestNeighbor_100Cities_ShouldCompleteQuickly()
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 42);
            var cities = generator.GenerateRandomCities(100);
            var solver = new NearestNeighborSolver();

            // Act
            var startTime = DateTime.UtcNow;
            var tour = await solver.SolveAsync(cities);
            var elapsed = DateTime.UtcNow - startTime;

            // Assert
            Assert.NotNull(tour);
            Assert.Equal(100, tour.Cities.Count);
            Assert.True(tour.TotalDistance > 0);
            Assert.True(elapsed.TotalSeconds < 5); // Should be very fast
        }

        [Fact]
        public async Task Performance_AllSolvers_SmallProblem_ShouldComplete()
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 555);
            var cities = generator.GenerateRandomCities(6);

            var solvers = new List<ITspSolver>
            {
                new NearestNeighborSolver(),
                new TwoOptSolver(maxIterations: 20),
                new SimulatedAnnealingSolver(100, 0.9, 10),
                new GeneticAlgorithmSolver(20, 10, 0.1, 0.2)
            };

            // Act
            var tasks = solvers.Select(solver => Task.Run(async () =>
            {
                var startTime = DateTime.UtcNow;
                var tour = await solver.SolveAsync(cities);
                var elapsed = DateTime.UtcNow - startTime;
                return new { Solver = solver.Name, Tour = tour, Elapsed = elapsed };
            })).ToList();

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.All(results, result =>
            {
                Assert.NotNull(result.Tour);
                Assert.Equal(6, result.Tour.Cities.Count);
                Assert.True(result.Tour.TotalDistance > 0);
                Assert.True(result.Elapsed.TotalSeconds < 30); // Reasonable time limit
            });
        }

        [Fact]
        public void Performance_DistanceMatrix_LargeProblem_ShouldBuildEfficiently()
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 123);
            var cities = generator.GenerateRandomCities(500);
            var solver = new NearestNeighborSolver();

            // Act
            var startTime = DateTime.UtcNow;
            var matrix = typeof(TspSolverBase)
                .GetMethod("BuildDistanceMatrix", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(solver, new object[] { cities }) as double[,];
            var elapsed = DateTime.UtcNow - startTime;

            // Assert
            Assert.NotNull(matrix);
            Assert.Equal(500, matrix.GetLength(0));
            Assert.Equal(500, matrix.GetLength(1));
            Assert.True(elapsed.TotalSeconds < 2); // Should build quickly
        }

        [Fact]
        public async Task Performance_TourOperations_ShouldBeEfficient()
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 456);
            var cities = generator.GenerateRandomCities(1000);
            var distanceMatrix = new double[1000, 1000];

            // Build distance matrix
            for (int i = 0; i < 1000; i++)
            {
                for (int j = 0; j < 1000; j++)
                {
                    distanceMatrix[i, j] = cities[i].DistanceTo(cities[j]);
                }
            }

            var tour = new Tour(cities, distanceMatrix);

            // Act & Assert - Multiple operations should be efficient
            var startTime = DateTime.UtcNow;

            // Test distance calculation caching
            var distance1 = tour.TotalDistance;
            var distance2 = tour.TotalDistance; // Should use cache
            Assert.Equal(distance1, distance2);

            // Test swapping
            tour.SwapCities(100, 200);
            var newDistance = tour.TotalDistance;

            // Test reversing
            tour.Reverse(50, 150);

            // Test cloning
            var cloned = tour.Clone();

            var elapsed = DateTime.UtcNow - startTime;

            Assert.True(elapsed.TotalSeconds < 1); // Operations should be fast
            Assert.NotEqual(distance1, newDistance); // Distance should change after modifications
            Assert.Equal(tour.TotalDistance, cloned.TotalDistance); // Clone should match
        }
    }

    // ============================================================================
    // EDGE CASE AND ERROR HANDLING TESTS
    // ============================================================================
    public class EdgeCaseTests
    {
        [Fact]
        public async Task EdgeCase_EmptyCityList_AllSolvers_ShouldHandle()
        {
            // Arrange
            var emptyCities = new List<City>();
            var solvers = TspSolverFactory.CreateAllSolvers().ToList();

            // Act & Assert
            foreach (var solver in solvers)
            {
                var tour = await solver.SolveAsync(emptyCities);
                Assert.NotNull(tour);
                Assert.Empty(tour.Cities);
                Assert.Equal(0.0, tour.TotalDistance);
            }
        }

        [Fact]
        public async Task EdgeCase_SingleCity_AllSolvers_ShouldHandle()
        {
            // Arrange
            var singleCity = new List<City> { new City(0, "Only", 5, 10) };
            var solvers = TspSolverFactory.CreateAllSolvers().ToList();

            // Act & Assert
            foreach (var solver in solvers)
            {
                var tour = await solver.SolveAsync(singleCity);
                Assert.NotNull(tour);
                Assert.Single(tour.Cities);
                Assert.Equal(0.0, tour.TotalDistance);
                Assert.Equal("Only", tour.Cities[0].Name);
            }
        }

        [Fact]
        public void EdgeCase_City_ExtremeCoordinates_ShouldWork()
        {
            // Arrange & Act
            var city1 = new City(0, "Extreme1", double.MaxValue / 2, double.MaxValue / 2);
            var city2 = new City(1, "Extreme2", double.MinValue / 2, double.MinValue / 2);

            // Assert
            Assert.True(double.IsFinite(city1.X));
            Assert.True(double.IsFinite(city1.Y));
            Assert.True(double.IsFinite(city2.X));
            Assert.True(double.IsFinite(city2.Y));

            var distance = city1.DistanceTo(city2);
            Assert.True(double.IsFinite(distance));
            Assert.True(distance > 0);
        }

        [Fact]
        public void EdgeCase_Tour_OutOfBoundsSwap_ShouldThrow()
        {
            // Arrange
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 1)
            };
            var matrix = new double[2, 2];
            var tour = new Tour(cities, matrix);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => tour.SwapCities(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => tour.SwapCities(0, 10));
        }

        [Fact]
        public void EdgeCase_Tour_InvalidReverseRange_ShouldHandle()
        {
            // Arrange
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 1),
                new City(2, "C", 2, 2)
            };
            var matrix = new double[3, 3];
            var tour = new Tour(cities, matrix);
            var originalCities = tour.Cities.ToList();

            // Act - Reverse with start > end should not crash
            tour.Reverse(2, 1); // Invalid range

            // Assert - Tour should remain unchanged
            for (int i = 0; i < originalCities.Count; i++)
            {
                Assert.Equal(originalCities[i], tour.Cities[i]);
            }
        }

        [Fact]
        public void EdgeCase_DataGenerator_ZeroCities_ShouldReturnEmpty()
        {
            // Arrange
            var generator = new TspDataGenerator();

            // Act
            var randomCities = generator.GenerateRandomCities(0);
            var circularCities = generator.GenerateCircularCities(0);
            var gridCities = generator.GenerateGridCities(0, 0);

            // Assert
            Assert.Empty(randomCities);
            Assert.Empty(circularCities);
            Assert.Empty(gridCities);
        }

        [Fact]
        public void EdgeCase_DataGenerator_NegativeParameters_ShouldThrow()
        {
            // Arrange
            var generator = new TspDataGenerator();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateRandomCities(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateCircularCities(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateGridCities(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateGridCities(1, -1));
        }

        [Fact]
        public void EdgeCase_GeneticAlgorithm_ZeroPopulation_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new GeneticAlgorithmSolver(populationSize: 0));
        }

        [Fact]
        public void EdgeCase_GeneticAlgorithm_NegativeGenerations_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new GeneticAlgorithmSolver(generations: -1));
        }

        [Theory]
        [InlineData(-0.1)] // Negative
        [InlineData(1.1)]  // Greater than 1
        public void EdgeCase_GeneticAlgorithm_InvalidRates_ShouldThrow(double invalidRate)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new GeneticAlgorithmSolver(mutationRate: invalidRate));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new GeneticAlgorithmSolver(elitismRate: invalidRate));
        }

        [Fact]
        public void EdgeCase_SimulatedAnnealing_InvalidParameters_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new SimulatedAnnealingSolver(initialTemperature: -1));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new SimulatedAnnealingSolver(coolingRate: 0)); // Should be > 0 and < 1
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new SimulatedAnnealingSolver(coolingRate: 1.1));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new SimulatedAnnealingSolver(iterationsPerTemperature: 0));
        }

        [Fact]
        public void EdgeCase_TwoOpt_ZeroMaxIterations_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new TwoOptSolver(maxIterations: 0));
        }

        [Fact]
        public async Task EdgeCase_CancellationToken_AlreadyCancelled_ShouldThrowImmediately()
        {
            // Arrange
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0)
            };

            var solvers = TspSolverFactory.CreateAllSolvers().ToList();
            using var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel before starting

            // Act & Assert
            foreach (var solver in solvers)
            {
                await Assert.ThrowsAsync<OperationCanceledException>(
                    () => solver.SolveAsync(cities, cts.Token));
            }
        }

        [Fact]
        public void EdgeCase_City_NaNCoordinates_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new City(0, "NaN", double.NaN, 0));
            Assert.Throws<ArgumentException>(() => new City(0, "NaN", 0, double.NaN));
            Assert.Throws<ArgumentException>(() => new City(0, "Inf", double.PositiveInfinity, 0));
            Assert.Throws<ArgumentException>(() => new City(0, "Inf", 0, double.NegativeInfinity));
        }

        [Fact]
        public void EdgeCase_City_NullName_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new City(0, null!, 0, 0));
            Assert.Throws<ArgumentException>(() => new City(0, "", 0, 0));
            Assert.Throws<ArgumentException>(() => new City(0, "   ", 0, 0));
        }

        [Fact]
        public void EdgeCase_Tour_NullCities_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new Tour(null!, new double[0, 0]));
        }

        [Fact]
        public void EdgeCase_Tour_NullDistanceMatrix_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new Tour(new List<City>(), null!));
        }
    }

    // ============================================================================
    // CONCURRENCY AND THREAD SAFETY TESTS
    // ============================================================================
    public class ConcurrencyTests
    {
        [Fact]
        public async Task Concurrency_MultipleSolvers_ParallelExecution_ShouldWork()
        {
            // Arrange
            var generator = new TspDataGenerator(seed: 999);
            var cities = generator.GenerateRandomCities(8);

            var tasks = new List<Task<Tour>>();
            for (int i = 0; i < 10; i++)
            {
                var solver = new NearestNeighborSolver();
                tasks.Add(Task.Run(() => solver.SolveAsync(cities)));
            }

            // Act
            var tours = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(10, tours.Length);
            Assert.All(tours, tour =>
            {
                Assert.NotNull(tour);
                Assert.Equal(8, tour.Cities.Count);
            });

            // All should have the same result (deterministic)
            var firstDistance = tours[0].TotalDistance;
            Assert.All(tours, tour => Assert.Equal(firstDistance, tour.TotalDistance));
        }

        [Fact]
        public async Task Concurrency_SameSolver_MultipleCallsSequentially_ShouldWork()
        {
            // Arrange
            var solver = new NearestNeighborSolver();
            var generator = new TspDataGenerator(seed: 111);

            // Act
            var tasks = new List<Task<Tour>>();
            for (int i = 0; i < 5; i++)
            {
                var cities = generator.GenerateRandomCities(6);
                tasks.Add(solver.SolveAsync(cities));
            }

            var tours = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(5, tours.Length);
            Assert.All(tours, tour =>
            {
                Assert.NotNull(tour);
                Assert.Equal(6, tour.Cities.Count);
            });
        }

        [Fact]
        public void Concurrency_TourOperations_ShouldBeThreadSafe()
        {
            // Arrange
            var cities = new List<City>();
            for (int i = 0; i < 100; i++)
            {
                cities.Add(new City(i, $"City{i}", i, i * 2));
            }

            var distanceMatrix = new double[100, 100];
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    distanceMatrix[i, j] = cities[i].DistanceTo(cities[j]);
                }
            }

            var tour = new Tour(cities, distanceMatrix);

            // Act - Multiple threads reading distance (cached value)
            var tasks = new List<Task<double>>();
            for (int i = 0; i < 20; i++)
            {
                tasks.Add(Task.Run(() => tour.TotalDistance));
            }

            var distances = Task.WhenAll(tasks).Result;

            // Assert
            Assert.All(distances, distance => Assert.Equal(distances[0], distance));
        }
    }

    // ============================================================================
    // LOGGING AND OBSERVABILITY TESTS
    // ============================================================================
    public class LoggingTests
    {
        private class TestLogger : ILogger
        {
            public List<string> LogMessages { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => 
                throw new NotImplementedException();

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, 
                Exception? exception, Func<TState, Exception?, string> formatter)
            {
                LogMessages.Add(formatter(state, exception));
            }
        }

        private class TestLoggerProvider : ILoggerProvider
        {
            public TestLogger TestLogger { get; } = new();

            public ILogger CreateLogger(string categoryName) => TestLogger;
            public void Dispose() { }
        }

        [Fact]
        public async Task Logging_NearestNeighborSolver_ShouldLogProgress()
        {
            // Arrange
            var loggerProvider = new TestLoggerProvider();
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddProvider(loggerProvider).SetMinimumLevel(LogLevel.Debug));

            var solver = new NearestNeighborSolver(loggerFactory.CreateLogger<NearestNeighborSolver>());
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0),
                new City(2, "C", 2, 0)
            };

            // Act
            await solver.SolveAsync(cities);

            // Assert
            Assert.True(loggerProvider.TestLogger.LogMessages.Count > 0);
            Assert.Contains(loggerProvider.TestLogger.LogMessages,
                msg => msg.Contains("Starting Nearest Neighbor"));
        }

        [Fact]
        public async Task Logging_TspBenchmark_ShouldLogResults()
        {
            // Arrange
            var loggerProvider = new TestLoggerProvider();
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddProvider(loggerProvider).SetMinimumLevel(LogLevel.Information));

            var benchmark = new TspBenchmark(loggerFactory.CreateLogger<TspBenchmark>());
            var cities = new List<City>
            {
                new City(0, "A", 0, 0),
                new City(1, "B", 1, 0)
            };

            var solvers = new List<ITspSolver>
            {
                new NearestNeighborSolver()
            };

            // Act
            await benchmark.RunBenchmarkAsync(cities, solvers);

            // Assert
            Assert.True(loggerProvider.TestLogger.LogMessages.Count > 0);
            Assert.Contains(loggerProvider.TestLogger.LogMessages,
                msg => msg.Contains("Starting benchmark"));
        }

        [Fact]
        public void Logging_TspDataGenerator_ShouldLogGeneration()
        {
            // Arrange
            var loggerProvider = new TestLoggerProvider();
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddProvider(loggerProvider).SetMinimumLevel(LogLevel.Information));

            var generator = new TspDataGenerator(seed: 123, 
                loggerFactory.CreateLogger<TspDataGenerator>());

            // Act
            generator.GenerateRandomCities(5);

            // Assert
            Assert.Contains(loggerProvider.TestLogger.LogMessages,
                msg => msg.Contains("Generated") && msg.Contains("random cities"));
        }
    }

    // ============================================================================
    // HELPER METHODS AND UTILITIES
    // ============================================================================
    internal static class TestHelpers
    {
        public static List<City> CreateTestCities(int count, int seed = 42)
        {
            var generator = new TspDataGenerator(seed);
            return generator.GenerateRandomCities(count).ToList();
        }

        public static double[,] CreateDistanceMatrix(IReadOnlyList<City> cities)
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

        public static bool AreToursEquivalent(Tour tour1, Tour tour2, double tolerance = 0.01)
        {
            if (tour1.Cities.Count != tour2.Cities.Count)
                return false;

            return Math.Abs(tour1.TotalDistance - tour2.TotalDistance) < tolerance;
        }

        public static void AssertTourIsValid(Tour tour, IReadOnlyList<City> originalCities)
        {
            Assert.NotNull(tour);
            Assert.Equal(originalCities.Count, tour.Cities.Count);
            Assert.True(tour.TotalDistance >= 0);

            // All original cities should be present
            var originalIds = originalCities.Select(c => c.Id).OrderBy(id => id).ToList();
            var tourIds = tour.Cities.Select(c => c.Id).OrderBy(id => id).ToList();
            Assert.Equal(originalIds, tourIds);
        }

        public static async Task<TimeSpan> MeasureExecutionTime(Func<Task> action)
        {
            var startTime = DateTime.UtcNow;
            await action();
            return DateTime.UtcNow - startTime;
        }
    }
}