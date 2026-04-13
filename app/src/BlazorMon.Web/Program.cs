using MudBlazor.Services;
using BlazorMon.Application.BackgroundServices;
using BlazorMon.Application.Services;
using BlazorMon.Infrastructure;
using BlazorMon.Infrastructure.Identity;
using BlazorMon.Web.Components;
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
    // When no SQL Server connection string is configured the app falls back to
    // a local SQLite database (blazormon.db) — useful for development / demo.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var pluginsDir = builder.Configuration["PluginsDirectory"]
        ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "plugins");
    pluginsDir = Path.GetFullPath(pluginsDir);

    if (string.IsNullOrWhiteSpace(connectionString))
        Log.Warning("No SQL Server connection string configured — using SQLite (blazormon.db)");

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

    // Ensure the database schema exists.
    // For SQLite (no connection string configured) EnsureCreated() creates the
    // file and all tables automatically on first run.
    // For SQL Server the user is expected to run app/db/init.sql manually,
    // so we only call EnsureCreated() when the provider is SQLite.
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BlazorMon.Infrastructure.Data.PolyMonDbContext>();
        if (db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
            await db.Database.EnsureCreatedAsync();
    }

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

    const string defaultEmail = "admin@blazormon.local";
    const string defaultPassword = "BlazorMon1!";

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
