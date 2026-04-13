using MudBlazor.Services;
using PolyMon.Application.BackgroundServices;
using PolyMon.Application.Services;
using PolyMon.Infrastructure;
using PolyMon.Infrastructure.Identity;
using PolyMon.Web.Components;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/polymon-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Enable running as a Windows Service (no-op on non-Windows / non-service environments)
    builder.Host.UseWindowsService();

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .WriteTo.File("logs/polymon-.log", rollingInterval: RollingInterval.Day));

    // Infrastructure (EF Core, repositories, Identity, plugin scanner, email)
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
    builder.Services.AddHostedService<AggregationService>();

    // Razor Pages (used for Login / Logout — must run outside SignalR)
    builder.Services.AddRazorPages();

    // Cascading auth state for Blazor components
    builder.Services.AddCascadingAuthenticationState();

    // Blazor
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // MudBlazor
    builder.Services.AddMudServices();

    var app = builder.Build();

    // Seed a default admin user on first run
    await SeedAdminUserAsync(app.Services);

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();
    app.MapStaticAssets();
    app.MapRazorPages();
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

// ---------------------------------------------------------------------------
// Seed a default admin account on first startup (no users in DB yet)
// ---------------------------------------------------------------------------
static async Task SeedAdminUserAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();

    const string defaultEmail = "admin@polymon.local";
    const string defaultPassword = "PolyMon1!";

    if (await userManager.FindByEmailAsync(defaultEmail) is null)
    {
        var user = new ApplicationUser
        {
            UserName = defaultEmail,
            Email = defaultEmail,
            DisplayName = "Administrator",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, defaultPassword);

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(
            "Default admin user created ({Email}). Change the password immediately via Admin → Users.",
            defaultEmail);
    }
}
