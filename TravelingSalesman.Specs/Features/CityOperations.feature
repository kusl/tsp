Feature: City Operations
    As a developer using the TSP library
    I want to work with city objects and calculate distances
    So that I can model traveling salesman problems

@city
Scenario: Calculate distance between two cities
    Given I have a city "A" at coordinates (0, 0)
    And I have a city "B" at coordinates (3, 4)
    When I calculate the distance from city "A" to city "B"
    Then the distance should be 5.0 units

@city
Scenario: Cities at the same location have zero distance
    Given I have a city "A" at coordinates (10.5, 20.3)
    When I calculate the distance from city "A" to itself
    Then the distance should be 0.0 units

@city @tour
Scenario: Tour calculates total distance correctly
    Given I have the following cities in order:
        | Name | X | Y |
        | A    | 0 | 0 |
        | B    | 1 | 0 |
        | C    | 1 | 1 |
        | D    | 0 | 1 |
    When I create a tour visiting cities in the order A, B, C, D
    Then the total tour distance should be 4.0 units

@city @tour
Scenario: Empty tour has zero distance
    Given I have no cities
    When I create an empty tour
    Then the total tour distance should be 0.0 units
