using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll;
using TravelingSalesman.Core;
using Xunit;

namespace TravelingSalesman.Specs.StepDefinitions
{
    [Binding]
    public class DataGenerationSteps
    {
        private IReadOnlyList&lt;City&gt; _generatedCities = new List&lt;City&gt;();
        private IReadOnlyList&lt;City&gt; _secondGeneratedCities = new List&lt;City&gt;();

        [When(@"I generate (.*) random cities with seed (.*)")]
        public void WhenIGenerateRandomCitiesWithSeed(int count, int seed)
        {
            var generator = new TspDataGenerator(seed);
            _generatedCities = generator.GenerateRandomCities(count);
        }

        [When(@"I generate (.*) random cities again with seed (.*)")]
        public void WhenIGenerateRandomCitiesAgainWithSeed(int count, int seed)
        {
            var generator = new TspDataGenerator(seed);
            _secondGeneratedCities = generator.GenerateRandomCities(count);
        }

        [When(@"I generate (.*) cities in a circular pattern with radius (.*)")]
        public void WhenIGenerateCitiesInACircularPatternWithRadius(int count, double radius)
        {
            var generator = new TspDataGenerator();
            _generatedCities = generator.GenerateCircularCities(count, radius);
        }

        [When(@"I generate a (.*)x(.*) grid of cities with spacing (.*)")]
        public void WhenIGenerateAGridOfCitiesWithSpacing(int rows, int cols, double spacing)
        {
            var generator = new TspDataGenerator();
            _generatedCities = generator.GenerateGridCities(rows, cols, spacing);
        }

        [Then(@"I should have (.*) cities")]
        public void ThenIShouldHaveCities(int expectedCount)
        {
            Assert.Equal(expectedCount, _generatedCities.Count);
        }

        [Then(@"all cities should be within bounds \((.*),(.*)\) to \((.*),(.*)\)")]
        public void ThenAllCitiesShouldBeWithinBounds(double minX, double minY, double maxX, double maxY)
        {
            Assert.All(_generatedCities, city =>
            {
                Assert.InRange(city.X, minX, maxX);
                Assert.InRange(city.Y, minY, maxY);
            });
        }

        [Then(@"all cities should have unique IDs from (.*) to (.*)")]
        public void ThenAllCitiesShouldHaveUniqueIDsFromTo(int minId, int maxId)
        {
            var ids = _generatedCities.Select(c => c.Id).ToList();
            
            // Check uniqueness
            Assert.Equal(ids.Count, ids.Distinct().Count());
            
            // Check range
            Assert.Equal(minId, ids.Min());
            Assert.Equal(maxId, ids.Max());
            
            // Check sequential
            var sortedIds = ids.OrderBy(id => id).ToList();
            for (int i = 0; i < sortedIds.Count; i++)
            {
                Assert.Equal(minId + i, sortedIds[i]);
            }
        }

        [Then(@"all cities should be approximately (.*) units from center \((.*),(.*)\)")]
        public void ThenAllCitiesShouldBeApproximatelyUnitsFromCenter(double radius, double centerX, double centerY)
        {
            Assert.All(_generatedCities, city =>
            {
                var distance = Math.Sqrt(Math.Pow(city.X - centerX, 2) + Math.Pow(city.Y - centerY, 2));
                Assert.Equal(radius, distance, 1); // 1 unit tolerance
            });
        }

        [Then(@"the cities should be evenly distributed around the circle")]
        public void ThenTheCitiesShouldBeEvenlyDistributedAroundTheCircle()
        {
            // Calculate angles between consecutive cities
            var centerX = 50.0;
            var centerY = 50.0;
            
            var angles = new List&lt;double&gt;();
            foreach (var city in _generatedCities)
            {
                var angle = Math.Atan2(city.Y - centerY, city.X - centerX);
                angles.Add(angle);
            }
            
            angles.Sort();
            
            // Check that angles are evenly spaced
            var expectedAngleStep = 2 * Math.PI / _generatedCities.Count;
            
            for (int i = 1; i < angles.Count; i++)
            {
                var angleDiff = angles[i] - angles[i - 1];
                Assert.Equal(expectedAngleStep, angleDiff, 0.1); // 0.1 radian tolerance
            }
        }

        [Then(@"the cities should form a regular grid pattern")]
        public void ThenTheCitiesShouldFormARegularGridPattern()
        {
            // Cities should have regular X and Y coordinates
            var xCoords = _generatedCities.Select(c => c.X).Distinct().OrderBy(x => x).ToList();
            var yCoords = _generatedCities.Select(c => c.Y).Distinct().OrderBy(y => y).ToList();
            
            // Check regular spacing in X
            if (xCoords.Count > 1)
            {
                var xSpacing = xCoords[1] - xCoords[0];
                for (int i = 2; i < xCoords.Count; i++)
                {
                    Assert.Equal(xSpacing, xCoords[i] - xCoords[i - 1], 0.01);
                }
            }
            
            // Check regular spacing in Y
            if (yCoords.Count > 1)
            {
                var ySpacing = yCoords[1] - yCoords[0];
                for (int i = 2; i < yCoords.Count; i++)
                {
                    Assert.Equal(ySpacing, yCoords[i] - yCoords[i - 1], 0.01);
                }
            }
        }

        [Then(@"the minimum distance between adjacent cities should be (.*) units")]
        public void ThenTheMinimumDistanceBetweenAdjacentCitiesShouldBeUnits(double expectedDistance)
        {
            // For a grid, adjacent cities are those with minimum non-zero distance
            var distances = new List&lt;double&gt;();
            
            for (int i = 0; i < _generatedCities.Count; i++)
            {
                for (int j = i + 1; j < _generatedCities.Count; j++)
                {
                    var distance = _generatedCities[i].DistanceTo(_generatedCities[j]);
                    if (distance > 0)
                    {
                        distances.Add(distance);
                    }
                }
            }
            
            var minDistance = distances.Min();
            Assert.Equal(expectedDistance, minDistance, 0.01);
        }

        [Then(@"both city sets should be identical")]
        public void ThenBothCitySetsShouldBeIdentical()
        {
            Assert.Equal(_generatedCities.Count, _secondGeneratedCities.Count);
            
            for (int i = 0; i < _generatedCities.Count; i++)
            {
                var city1 = _generatedCities[i];
                var city2 = _secondGeneratedCities[i];
                
                Assert.Equal(city1.Id, city2.Id);
                Assert.Equal(city1.Name, city2.Name);
                Assert.Equal(city1.X, city2.X, 10); // High precision
                Assert.Equal(city1.Y, city2.Y, 10);
            }
        }
    }
}
