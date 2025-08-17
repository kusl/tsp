using Reqnroll;
using System;

namespace TravelingSalesman.Specs.Support
{
    [Binding]
    public class Hooks
    {
        [BeforeScenario]
        public void BeforeScenario(ScenarioContext scenarioContext)
        {
            // Initialize any test data or services needed
            Console.WriteLine($"Starting scenario: {scenarioContext.ScenarioInfo.Title}");
        }

        [AfterScenario]
        public void AfterScenario(ScenarioContext scenarioContext)
        {
            // Clean up any resources
            Console.WriteLine($"Completed scenario: {scenarioContext.ScenarioInfo.Title}");
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Global setup
            Console.WriteLine("Starting TSP Reqnroll test run");
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            // Global cleanup
            Console.WriteLine("Completed TSP Reqnroll test run");
        }
    }
}
