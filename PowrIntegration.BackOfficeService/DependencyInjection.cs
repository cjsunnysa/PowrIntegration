using Microsoft.EntityFrameworkCore;
using PowrIntegration.BackOfficeService.Data;
using PowrIntegration.BackOfficeService.Options;
using PowrIntegration.Shared.Options;

namespace PowrIntegration.BackOfficeService;

internal static class DependencyInjection
{
    public static IServiceCollection ConfigureEntityFramework(this IServiceCollection services, string connectionString)
    {
        return services.AddDbContextFactory<PowrIntegrationDbContext>(options => options.UseSqlite(connectionString));
    }

    public static IServiceCollection ConfigureServiceOptions(this IServiceCollection services, ConfigurationManager config)
    {
        services.Configure<BackOfficeServiceOptions>(config.GetSection(ServiceOptions.KEY));
        services.Configure<RabbitMqOptions>(config.GetSection(RabbitMqOptions.KEY));
        services.Configure<PowertillOptions>(config.GetSection(PowertillOptions.KEY));
        services.Configure<ZraApiOptions>(config.GetSection(ZraApiOptions.KEY));

        return services;
    }

    public static IServiceProvider ApplyDatabaseMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<PowrIntegrationDbContext>();

            context.Database.Migrate(); // Create or update the database schema
        }
        catch (Exception ex)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            var logger = loggerFactory.CreateLogger("Startup");

            logger.LogError(ex, "An error ocurred attempting to apply database migrations.");
        }

        return serviceProvider;
    }
}
