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
// Razor + Blazor
// --------------------------------------------------
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
// DATABASE (SQLite – Render/Docker safe)
// --------------------------------------------------
// Use persistent disk if provided (Render), otherwise fallback to /app/Data
var sqliteDir = Environment.GetEnvironmentVariable("SQLITE_DIR");

if (string.IsNullOrWhiteSpace(sqliteDir))
{
    sqliteDir = Path.Combine(AppContext.BaseDirectory, "Data");
}

// Ensure directory exists and is writable
Directory.CreateDirectory(sqliteDir);

// Absolute DB path
var sqlitePath = Path.Combine(sqliteDir, "app.db");
var connectionString = $"Data Source={sqlitePath}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

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
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

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

// Identity endpoints
app.MapAdditionalIdentityEndpoints();

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