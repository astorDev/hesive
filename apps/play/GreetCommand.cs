using Microsoft.Extensions.Logging;

namespace Playground;

public class GreetCommand : Command
{
    private readonly Option<string> nameOption = new("--name")
    {
        Description = "The name of the person to greet.",
        Required = true
    };

    private readonly Argument<string> pathArgument = new("path")
    {
        Description = "The path from which to read files.",
        Arity = ArgumentArity.ExactlyOne
    };
    
    private readonly ILogger<GreetCommand> logger;

    public GreetCommand(ILogger<GreetCommand> logger) : base("greet", "Greet a person by name.")
    {
        Add(pathArgument);
        Add(nameOption);
        SetAction(Execute);

        this.logger = logger;
    }

    private void Execute(ParseResult parseResult)
    {
        var name = parseResult.GetRequiredValue(nameOption);
        var path = parseResult.GetRequiredValue(pathArgument);

        logger.LogDebug("Greeting {Name}...", name);
        Console.WriteLine($"Hello, {name}!");
        logger.LogInformation("Greeted {Name} successfully.", name);

        logger.LogDebug("Getting entries in the path: `{Path}`", path);
        var entryNames = Directory.GetFileSystemEntries(path)
            .Select(Path.GetFileName)
            .ToList();
        logger.LogInformation("Found {Count} entries in the path: `{Path}`. Writing to the output...", entryNames.Count, path);

        entryNames.ForEach(Console.WriteLine);
    }
}