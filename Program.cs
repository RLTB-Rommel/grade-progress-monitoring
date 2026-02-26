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
// DATABASE (Neon Postgres or SQLite fallback)
// --------------------------------------------------
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection") ??
    Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    var sqlitePath = Path.Combine(AppContext.BaseDirectory, "app.db");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite($"Data Source={sqlitePath}"));
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