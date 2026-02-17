using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using GradeProgressMonitoring.Components;
using GradeProgressMonitoring.Components.Account;
using GradeProgressMonitoring.Data;
using GradeProgressMonitoring.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddScoped<GradingTemplateService>();

// No real email sender yet (dev)
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Ensure DB is migrated before seeding roles/users
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Seed roles + admin user
await IdentitySeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
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

//
// Custom Logout Endpoint (avoids /Account/Logout returnUrl issues)
// Use this endpoint from NavMenu: action="/logout"
//
app.MapPost("/logout", async (
    HttpContext httpContext,
    [FromForm] string? returnUrl,
    SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();

    // Only redirect to a local URL
    return Results.LocalRedirect(SanitizeLocalReturnUrl(returnUrl));
})
.RequireAuthorization(); // only logged-in users can hit it

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();

static string SanitizeLocalReturnUrl(string? returnUrl)
{
    if (string.IsNullOrWhiteSpace(returnUrl))
        return "/";

    returnUrl = returnUrl.Trim();

    // Reject absolute URLs
    if (Uri.TryCreate(returnUrl, UriKind.Absolute, out _))
        return "/";

    // Reject protocol-relative
    if (returnUrl.StartsWith("//", StringComparison.Ordinal))
        return "/";

    // Allow virtual local
    if (returnUrl.StartsWith("~/", StringComparison.Ordinal))
        return returnUrl;

    // Ensure leading slash
    if (!returnUrl.StartsWith("/", StringComparison.Ordinal))
        return "/" + returnUrl;

    return returnUrl;
}
