using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hesive;

public class AppBuilder
{
    public IServiceCollection Services { get; }

    public ConfigurationManager Configuration { get; }

    public LoggingBuilder Logging { get;}

    public AppBuilder()
    {
        Services = new ServiceCollection();

        Configuration = new ConfigurationManager();
        Services.AddSingleton<IConfiguration>(Configuration);

        Services.AddLogging();
        Logging = new LoggingBuilder(Services);
        Logging.AddConfiguration(Configuration.GetSection("Logging"));
    }

    public App Build()
    {
        var serviceProvider = Services.BuildServiceProvider();
        return App.From(serviceProvider);
    }
}

public record LoggingBuilder(IServiceCollection Services) : ILoggingBuilder;