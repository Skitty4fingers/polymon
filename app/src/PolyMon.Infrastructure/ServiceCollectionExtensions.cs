using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PolyMon.Application.Interfaces;
using PolyMon.Infrastructure.Data;
using PolyMon.Infrastructure.Plugins;
using PolyMon.Infrastructure.Repositories;
using PolyMon.Infrastructure.Services;

namespace PolyMon.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        string connectionString, string pluginsDirectory)
    {
        services.AddDbContext<PolyMonDbContext>(opts =>
            opts.UseSqlServer(connectionString));

        services.AddScoped<IMonitorRepository, MonitorRepository>();
        services.AddScoped<IMonitorTypeRepository, MonitorTypeRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IOperatorRepository, OperatorRepository>();
        services.AddScoped<ISysSettingsRepository, SysSettingsRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();

        services.AddSingleton<IPluginScanner>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<PluginScanner>>();
            var scanner = new PluginScanner(pluginsDirectory, logger);
            scanner.Reload();
            return scanner;
        });

        services.AddSingleton<IPowerShellPluginExecutor, PowerShellPluginExecutor>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();

        return services;
    }
}
