using Hesive;
using Microsoft.Extensions.DependencyInjection;

var builder = new AppBuilder();

builder.Logging.AddNiceShell();

builder.Services.AddAsRootCommand<GreetCommand>();

var app = builder.Build();
var rootCommand = app.ServiceProvider.GetRequiredService<RootCommand>();

var parsed = rootCommand.Parse(args);
return await parsed.InvokeAsync();

class CommandRootingProxy : RootCommand
{
    public CommandRootingProxy(Command wrapped, string description) : base(description)
    {
        foreach (var subcommand in wrapped.Subcommands) Subcommands.Add(subcommand);
        foreach (var option in wrapped.Options) Options.Add(option);
        foreach (var argument in wrapped.Arguments) Arguments.Add(argument);
        foreach (var alias in wrapped.Aliases) Aliases.Add(alias);

        Action = wrapped.Action;
        Hidden = wrapped.Hidden;
        TreatUnmatchedTokensAsErrors = wrapped.TreatUnmatchedTokensAsErrors;
    }
}

public static class ServiceCollectionExtensions
{
    public static void AddAsRootCommand<TCommand>(this IServiceCollection services, string? descriptionOverwrite = null) where TCommand : Command
    {
        services.AddSingleton<TCommand>();
        services.AddSingleton<RootCommand>(sp =>
        {
            var command = sp.GetRequiredService<TCommand>();
            var description = descriptionOverwrite ?? command.Description ?? throw new InvalidOperationException($"Command {typeof(TCommand).Name} has no description and no description override was provided.");
            
            return new CommandRootingProxy(command, description);
        });
    }
}