Feature: Cities and tours
  As a developer modelling routing problems
  I want reliable distance calculations
  So that solver results can be trusted

  @city
  Scenario: Distance between two cities
    Given a city "A" at 0.0, 0.0
    And a city "B" at 3.0, 4.0
    When I measure the distance from "A" to "B"
    Then the distance is 5.0 units

  @city
  Scenario: A city is no distance from itself
    Given a city "A" at 10.5, 20.3
    When I measure the distance from "A" to "A"
    Then the distance is 0.0 units

  @tour
  Scenario: A tour returns to its starting city
    Given the following cities:
      | Name | X | Y |
      | A    | 0 | 0 |
      | B    | 1 | 0 |
      | C    | 1 | 1 |
      | D    | 0 | 1 |
    When I visit the cities in the order "A, B, C, D"
    Then the tour distance is 4.0 units

  @tour
  Scenario: A crossing tour is longer than the perimeter
    Given the following cities:
      | Name | X | Y |
      | A    | 0 | 0 |
      | B    | 1 | 0 |
      | C    | 1 | 1 |
      | D    | 0 | 1 |
    When I visit the cities in the order "A, C, B, D"
    Then the tour distance is greater than 4.0 units

  @tour
  Scenario: An empty tour has no distance
    Given no cities
    When I visit the cities in the order ""
    Then the tour distance is 0.0 units
