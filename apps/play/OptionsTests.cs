using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Playground;

[TestClass]
public class OptionsTests
{
    [TestMethod]
    public void ConfigureOnly()
    {
        var services = new ServiceCollection();
        services.Configure<ExampleOptions>(options =>
        {
            options.Name = "TestName";
        });

        var sp = services.BuildServiceProvider();

        var options = sp.GetRequiredService<IOptions<ExampleOptions>>();
        var monitor = sp.GetRequiredService<IOptionsMonitor<ExampleOptions>>();
        var snapshot = sp.GetRequiredService<IOptionsSnapshot<ExampleOptions>>();

        options.Value.Name.ShouldBe("TestName");
        monitor.CurrentValue.Name.ShouldBe("TestName");
        snapshot.Value.Name.ShouldBe("TestName");

        Console.WriteLine($"Options: {options.Value.Name}, Monitor: {monitor.CurrentValue.Name}, Snapshot: {snapshot.Value.Name}");
    }

    [TestMethod]
    public void ConfigureOnlyWithIConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ExampleOptions:Name", "ConfigureOnlyWithIConfiguration" }
            })
            .Build();

        var services = new ServiceCollection();
        services.Configure<ExampleOptions>(configuration.GetSection("ExampleOptions"));

        var sp = services.BuildServiceProvider();

        var options = sp.GetRequiredService<IOptions<ExampleOptions>>();
        var monitor = sp.GetRequiredService<IOptionsMonitor<ExampleOptions>>();
        var snapshot = sp.GetRequiredService<IOptionsSnapshot<ExampleOptions>>();

        options.Value.Name.ShouldBe("ConfigureOnlyWithIConfiguration");
        monitor.CurrentValue.Name.ShouldBe("ConfigureOnlyWithIConfiguration");
        snapshot.Value.Name.ShouldBe("ConfigureOnlyWithIConfiguration");

        Console.WriteLine($"Options: {options.Value.Name}, Monitor: {monitor.CurrentValue.Name}, Snapshot: {snapshot.Value.Name}");
    }
}

public class ExampleOptions
{
    public required string Name { get; set; }
}