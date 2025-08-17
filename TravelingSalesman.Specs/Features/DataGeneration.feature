Feature: TSP Data Generation
    As a user of the TSP solver
    I want to generate different city patterns for testing
    So that I can evaluate algorithm performance on various problem types

@datagen
Scenario: Generate random cities
    When I generate 10 random cities with seed 42
    Then I should have 10 cities
    And all cities should be within bounds (0,0) to (100,100)
    And all cities should have unique IDs from 0 to 9

@datagen
Scenario: Generate circular city pattern
    When I generate 8 cities in a circular pattern with radius 10
    Then I should have 8 cities
    And all cities should be approximately 10 units from center (50,50)
    And the cities should be evenly distributed around the circle

@datagen
Scenario: Generate grid city pattern
    When I generate a 3x3 grid of cities with spacing 10
    Then I should have 9 cities
    And the cities should form a regular grid pattern
    And the minimum distance between adjacent cities should be 10 units

@datagen @deterministic
Scenario: Seeded generation is deterministic
    When I generate 5 random cities with seed 123
    And I generate 5 random cities again with seed 123
    Then both city sets should be identical
