using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
//   - Local dev: SQLite in /Data/app.db (unless you set a real Postgres conn string)
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

    // Neon often needs SSL; keep Trust Server Certificate to avoid cert chain issues
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
        options.UseNpgsql(postgresConn));
}
else
{
    // Local SQLite fallback
    var sqliteDir = Path.Combine(AppContext.BaseDirectory, "Data");
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
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
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
// Ensure DB exists + migrations applied
// --------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Seed roles and admin user
await IdentitySeeder.SeedAsync(app.Services);

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

// ✅ Required for Identity endpoints + RequireAuthorization()
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

// ✅ Identity UI Razor Pages support (safe even if you use endpoint-based Identity)
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

// ✅ Identity endpoints (/Account/Login, /Account/Register, etc.)
app.MapAdditionalIdentityEndpoints();

// ✅ Blazor app fallback LAST
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

    // Block absolute URLs
    if (Uri.TryCreate(returnUrl, UriKind.Absolute, out _))
        return "/";

    // Block protocol-relative URLs
    if (returnUrl.StartsWith("//", StringComparison.Ordinal))
        return "/";

    // Allow app-relative "~/" routes
    if (returnUrl.StartsWith("~/", StringComparison.Ordinal))
        return returnUrl;

    // Ensure it begins with "/"
    if (!returnUrl.StartsWith("/", StringComparison.Ordinal))
        return "/" + returnUrl;

    return returnUrl;
}