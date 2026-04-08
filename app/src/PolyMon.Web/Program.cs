using MudBlazor.Services;
using PolyMon.Application.BackgroundServices;
using PolyMon.Application.Services;
using PolyMon.Infrastructure;
using PolyMon.Web.Components;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/polymon-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .WriteTo.File("logs/polymon-.log", rollingInterval: RollingInterval.Day));

    // Infrastructure (EF Core, repositories, plugin scanner, email)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    var pluginsDir = builder.Configuration["PluginsDirectory"]
        ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "plugins");
    pluginsDir = Path.GetFullPath(pluginsDir);

    builder.Services.AddInfrastructure(connectionString, pluginsDir);

    // Application services
    builder.Services.AddScoped<AlertEvaluationService>();
    builder.Services.AddScoped<MonitorExecutionService>();

    // Background services
    builder.Services.AddHostedService<MonitorExecutiveService>();
    builder.Services.AddHostedService<SummaryNotificationService>();

    // Blazor
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // MudBlazor
    builder.Services.AddMudServices();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();
    app.UseAntiforgery();
    app.MapStaticAssets();
    app.MapRazorComponents<App>()
       .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
