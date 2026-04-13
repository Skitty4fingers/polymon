using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BlazorMon.Application.Interfaces;
using BlazorMon.Infrastructure.Data;
using BlazorMon.Infrastructure.Identity;
using BlazorMon.Infrastructure.Plugins;
using BlazorMon.Infrastructure.Repositories;
using BlazorMon.Infrastructure.Services;

namespace BlazorMon.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        string? connectionString, string pluginsDirectory)
    {
        var useSqlite = string.IsNullOrWhiteSpace(connectionString);

        services.AddDbContext<PolyMonDbContext>(opts =>
        {
            if (useSqlite)
                opts.UseSqlite("Data Source=blazormon.db");
            else
                opts.UseSqlServer(connectionString);
        });

        // ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<PolyMonDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IMonitorRepository, MonitorRepository>();
        services.AddScoped<IMonitorTypeRepository, MonitorTypeRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IOperatorRepository, OperatorRepository>();
        services.AddScoped<ISysSettingsRepository, SysSettingsRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IAggregationRepository, AggregationRepository>();

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
