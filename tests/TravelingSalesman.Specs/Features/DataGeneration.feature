Feature: Generating problems
  As a user of the solver
  I want to generate different city layouts
  So that I can evaluate algorithms on varied problems

  @generation
  Scenario: Random cities stay inside the map
    When I generate 50 random cities with seed 42
    Then there are 50 cities
    And every city lies between 0.0 and 100.0 on both axes
    And every city has a unique name

  @generation
  Scenario: Seeded generation is reproducible
    When I generate 5 random cities with seed 123
    And I generate 5 random cities again with seed 123
    Then both city sets are identical

  @generation
  Scenario: Circle layout
    When I generate 8 cities on a circle of radius 10.0
    Then there are 8 cities
    And every city is 10.0 units from 50.0, 50.0

  @generation
  Scenario: Grid layout
    When I generate a 3 by 3 grid with spacing 10.0
    Then there are 9 cities
    And the closest pair of cities is 10.0 units apart
