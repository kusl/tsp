using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using TravelingSalesman.Core;

namespace TravelingSalesman.Cli;

internal sealed class ProblemOptions
{
    public const int MinimumCities = 2;
    public const int MaximumCities = 5_000;
    public const int DefaultCities = 20;

    public ProblemOptions()
    {
        Cities.Validators.Add(static result =>
        {
            if (SingleToken(result) is { } token
                && int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                && value is < MinimumCities or > MaximumCities)
            {
                result.AddError($"--cities must be between {MinimumCities} and {MaximumCities}.");
            }
        });

        Pattern.Validators.Add(static result =>
        {
            if (SingleToken(result) is { } token
                && (!Enum.TryParse<CityPattern>(token, ignoreCase: true, out var pattern) || !Enum.IsDefined(pattern) || char.IsAsciiDigit(token[0])))
            {
                result.AddError($"--pattern must be one of: {string.Join(", ", Enum.GetNames<CityPattern>()).ToLowerInvariant()}.");
            }
        });
    }

    public Option<int> Cities { get; } = new("--cities", "-n")
    {
        Description = "Number of cities to generate.",
        DefaultValueFactory = static _ => DefaultCities,
    };

    public Option<CityPattern> Pattern { get; } = new("--pattern", "-p")
    {
        Description = "Layout of the generated cities.",
        DefaultValueFactory = static _ => CityPattern.Random,
    };

    public Option<int?> Seed { get; } = new("--seed", "-s")
    {
        Description = "Seed for reproducible city layouts and solver runs.",
    };

    public void AddTo(Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        command.Options.Add(Cities);
        command.Options.Add(Pattern);
        command.Options.Add(Seed);
    }

    public TspProblem CreateProblem(ParseResult parseResult)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        return new TspProblem(CityGenerator.Create(parseResult.GetValue(Pattern), parseResult.GetValue(Cities), parseResult.GetValue(Seed)));
    }

    private static string? SingleToken(OptionResult result) =>
        result.Tokens.Count == 1 && result.Tokens[0].Value.Length > 0 ? result.Tokens[0].Value : null;
}
