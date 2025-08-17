using System.Collections.Generic;
using System.Linq;
using Reqnroll;
using TravelingSalesman.Core;
using Xunit;

namespace TravelingSalesman.Specs.StepDefinitions
{
    [Binding]
    public class CityOperationSteps
    {
        private readonly Dictionary&lt;string, City&gt; _cities = new();
        private readonly List&lt;City&gt; _cityList = new();
        private double _calculatedDistance;
        private Tour? _tour;

        [Given(@"I have a city ""(.*)"" at coordinates \((.*), (.*)\)")]
        public void GivenIHaveACityAtCoordinates(string name, double x, double y)
        {
            var city = new City(_cities.Count, name, x, y);
            _cities[name] = city;
            _cityList.Add(city);
        }

        [Given(@"I have the following cities in order:")]
        public void GivenIHaveTheFollowingCitiesInOrder(Table table)
        {
            _cityList.Clear();
            _cities.Clear();
            
            foreach (var row in table.Rows)
            {
                var name = row["Name"];
                var x = double.Parse(row["X"]);
                var y = double.Parse(row["Y"]);
                
                var city = new City(_cityList.Count, name, x, y);
                _cities[name] = city;
                _cityList.Add(city);
            }
        }

        [Given(@"I have no cities")]
        public void GivenIHaveNoCities()
        {
            _cityList.Clear();
            _cities.Clear();
        }

        [When(@"I calculate the distance from city ""(.*)"" to city ""(.*)""")]
        public void WhenICalculateTheDistanceFromCityToCity(string fromCity, string toCity)
        {
            var from = _cities[fromCity];
            var to = _cities[toCity];
            _calculatedDistance = from.DistanceTo(to);
        }

        [When(@"I calculate the distance from city ""(.*)"" to itself")]
        public void WhenICalculateTheDistanceFromCityToItself(string cityName)
        {
            var city = _cities[cityName];
            _calculatedDistance = city.DistanceTo(city);
        }

        [When(@"I create a tour visiting cities in the order (.*)")]
        public void WhenICreateATourVisitingCitiesInTheOrder(string cityOrder)
        {
            var cityNames = cityOrder.Split(", ");
            var orderedCities = cityNames.Select(name => _cities[name]).ToList();
            
            var distanceMatrix = BuildDistanceMatrix(orderedCities);
            _tour = new Tour(orderedCities, distanceMatrix);
        }

        [When(@"I create an empty tour")]
        public void WhenICreateAnEmptyTour()
        {
            var distanceMatrix = new double[0, 0];
            _tour = new Tour(new List&lt;City&gt;(), distanceMatrix);
        }

        [Then(@"the distance should be (.*) units")]
        public void ThenTheDistanceShouldBeUnits(double expectedDistance)
        {
            Assert.Equal(expectedDistance, _calculatedDistance, 1);
        }

        [Then(@"the total tour distance should be (.*) units")]
        public void ThenTheTotalTourDistanceShouldBeUnits(double expectedDistance)
        {
            Assert.NotNull(_tour);
            Assert.Equal(expectedDistance, _tour.TotalDistance, 1);
        }

        private static double[,] BuildDistanceMatrix(IReadOnlyList&lt;City&gt; cities)
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
    }
}