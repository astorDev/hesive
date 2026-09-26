using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hesive;

public record App(IServiceProvider Services, ILogger<App> Logger, IConfiguration Configuration)
{
    public static App From(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<App>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        return new App(serviceProvider, logger, configuration);
    }
}