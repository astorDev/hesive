var builder = new CliBuilder();

builder.Logging.AddNiceShell();

builder.AddCommand<GreetCommand>();

using var app = builder.Build("A hesive.apps CLI application.");

return app.Run(args);