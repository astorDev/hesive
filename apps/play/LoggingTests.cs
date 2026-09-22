using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Playground;

[TestClass]
public class LoggingTests
{
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void JustLoggingBuilder()
    {
        var services = new ServiceCollection();
        var loggingBuilder = new LoggingBuilder(services);
        loggingBuilder.AddSimpleConsole(c => c.SingleLine = true);

        var sp = services.BuildServiceProvider();

        try
        {
            var logger = sp.GetRequiredService<ILogger<LoggingTests>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get logger: {ex.Message}");
            throw;
        }
    }

    [TestMethod]
    public void AddLoggingAndBuilder()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        var loggingBuilder = new LoggingBuilder(services);

        loggingBuilder.AddSimpleConsole(c => c.SingleLine = true);

        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<ILogger<LoggingTests>>();

        logger.LogInformation("This is a test log message.");
    }

    [TestMethod]
    public void ConfigurationAndLogLevels()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationManager();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        var loggingBuilder = new LoggingBuilder(services);

        loggingBuilder.AddSimpleConsole(c => c.SingleLine = true);
        loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));

        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Logging:LogLevel:Default"] = "Warning",
        });

        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<ILogger<LoggingTests>>();

        logger.LogInformation("This is an information log message.");
        logger.LogWarning("This is a warning log message.");

        logger.IsEnabled(LogLevel.Information).ShouldBe(false);
        logger.IsEnabled(LogLevel.Warning).ShouldBe(true);
    }
}

public record LoggingBuilder(IServiceCollection Services) : ILoggingBuilder;