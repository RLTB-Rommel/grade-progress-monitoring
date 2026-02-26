using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.IO;

using GradeProgressMonitoring.Components;
using GradeProgressMonitoring.Components.Account;
using GradeProgressMonitoring.Data;
using GradeProgressMonitoring.Services;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Razor Pages + Blazor
// --------------------------------------------------
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --------------------------------------------------
// Authentication / Identity
// --------------------------------------------------
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

// --------------------------------------------------
// DATABASE
//   - Render/Neon: DATABASE_URL (postgresql://...)
//   - Local dev: SQLite in /Data/app.db
// --------------------------------------------------

static bool LooksLikePostgres(string s)
{
    s = s.Trim();
    return s.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        || s.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
        || s.Contains("Host=", StringComparison.OrdinalIgnoreCase);
}

static string BuildNpgsqlFromDatabaseUrl(string databaseUrl)
{
    // Accept either URI-style (postgresql://user:pass@host/db?sslmode=require)
    // or already-built Npgsql string (Host=...;Username=...;Password=...;Database=...)
    if (!databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return databaseUrl; // already an Npgsql-style connection string
    }

    var uri = new Uri(databaseUrl);

    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

    var database = uri.AbsolutePath.Trim('/');

    // Preserve sslmode from query if present (default to Require)
    var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
    var sslMode = query.TryGetValue("sslmode", out var ssl) ? ssl.ToString() : "Require";

    return
        $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};" +
        $"Database={database};Username={username};Password={password};" +
        $"SSL Mode={sslMode};Trust Server Certificate=true;Pooling=true;";
}

var configConn = builder.Configuration.GetConnectionString("DefaultConnection");
var envDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

string? postgresConn = null;

// Prefer DATABASE_URL on Render
if (!string.IsNullOrWhiteSpace(envDatabaseUrl))
{
    postgresConn = BuildNpgsqlFromDatabaseUrl(envDatabaseUrl);
}
else if (!string.IsNullOrWhiteSpace(configConn) && LooksLikePostgres(configConn))
{
    postgresConn = configConn;
}

if (!string.IsNullOrWhiteSpace(postgresConn))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(postgresConn)
    // ✅ Optional: log warnings instead of crashing if you want
    // .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning))
    );
}
else
{
    // Local SQLite fallback
    // IMPORTANT NOTE:
    // This writes to bin/.../Data/app.db (AppContext.BaseDirectory), not your project root.
    var sqliteDir = Path.Combine(builder.Environment.ContentRootPath, "Data");
    Directory.CreateDirectory(sqliteDir);

    var sqlitePath = Path.Combine(sqliteDir, "app.db");
    var sqliteConn = $"Data Source={sqlitePath};Cache=Shared";

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(sqliteConn));
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --------------------------------------------------
// Identity configuration
// --------------------------------------------------
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version2;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// --------------------------------------------------
// App Services
// --------------------------------------------------
builder.Services.AddScoped<GradingTemplateService>();

// Dev email sender (no-op)
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// --------------------------------------------------
// Ensure DB exists + migrations applied (SAFE on Render)
// --------------------------------------------------
await TryMigrateDbAsync(app);

// Seed roles and admin user (SAFE too)
await TrySeedIdentityAsync(app);

// --------------------------------------------------
// HTTP Pipeline
// --------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorPages();

// --------------------------------------------------
// Custom Logout Endpoint
// --------------------------------------------------
app.MapPost("/logout", async (
    HttpContext httpContext,
    [FromForm] string? returnUrl,
    SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect(SanitizeLocalReturnUrl(returnUrl));
})
.RequireAuthorization();

app.MapAdditionalIdentityEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// --------------------------------------------------
// Helpers
// --------------------------------------------------
static string SanitizeLocalReturnUrl(string? returnUrl)
{
    if (string.IsNullOrWhiteSpace(returnUrl))
        return "/";

    returnUrl = returnUrl.Trim();

    if (Uri.TryCreate(returnUrl, UriKind.Absolute, out _))
        return "/";

    if (returnUrl.StartsWith("//", StringComparison.Ordinal))
        return "/";

    if (returnUrl.StartsWith("~/", StringComparison.Ordinal))
        return returnUrl;

    if (!returnUrl.StartsWith("/", StringComparison.Ordinal))
        return "/" + returnUrl;

    return returnUrl;
}

static async Task TryMigrateDbAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                                     .CreateLogger("StartupMigrations");

    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

    // Retry helps Render/Neon where DB might not be ready yet
    const int maxAttempts = 5;
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();

            logger.LogInformation("Database migration completed.");
            return;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed (attempt {Attempt}/{Max}).", attempt, maxAttempts);

            // ✅ Don’t crash production container
            if (!env.IsDevelopment())
            {
                // Wait then retry
                await Task.Delay(TimeSpan.FromSeconds(3 * attempt));
                continue;
            }

            // In development, failing fast is OK
            throw;
        }
    }

    logger.LogWarning("Database migration could not be completed after retries. App will continue running.");
}

static async Task TrySeedIdentityAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                                     .CreateLogger("StartupSeeder");

    try
    {
        await IdentitySeeder.SeedAsync(app.Services);
        logger.LogInformation("Identity seeding completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Identity seeding failed.");
        // ✅ Don’t crash production container
    }
}