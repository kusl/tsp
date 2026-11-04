using TravelingSalesman.Cli;

return await CliApplication.CreateRootCommand(Console.Out, TimeProvider.System).Parse(args).InvokeAsync();
