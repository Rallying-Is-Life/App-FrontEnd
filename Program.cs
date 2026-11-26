using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// Azure AD / Entra External ID (CIAM) settings (from web-app settings, never committed)
// For CIAM: Instance = Authority = CIAM URL (e.g., https://tenant.ciamlogin.com/tenant/v2.0)
// For regular Entra ID: Instance = https://login.microsoftonline.com, Authority = organizations/v2.0 or tenant/v2.0
var tenantId = Environment.GetEnvironmentVariable("TenantID")
    ?? Environment.GetEnvironmentVariable("AzureAd__TenantId")
    ?? Environment.GetEnvironmentVariable("Auth__TenantId") ?? "";
var clientId = Environment.GetEnvironmentVariable("ClientID")
    ?? Environment.GetEnvironmentVariable("AzureAd__ClientId") ?? "";
var clientSecret = Environment.GetEnvironmentVariable("ClientSecret")
    ?? Environment.GetEnvironmentVariable("AzureAd__ClientSecret") ?? "";
var domain = Environment.GetEnvironmentVariable("AzureAd__Domain") ?? ""; // Required for CIAM
var ciamTenantName = Environment.GetEnvironmentVariable("CiamTenantName") ?? ""; // CIAM tenant subdomain

// Downstream API (custom API) configuration
var apiClientId = Environment.GetEnvironmentVariable("ApiClientId") ?? "";
var apiBaseUrl = Environment.GetEnvironmentVariable("ApiBaseUrl") ?? "";
var apiScopes = Environment.GetEnvironmentVariable("ApiScopes") ?? 
    (string.IsNullOrEmpty(apiClientId) ? "Api.Read,Api.Write" : $"api://{apiClientId}/Api.Read,api://{apiClientId}/Api.Write");

// Build Instance and Authority for CIAM vs regular Entra ID
// For CIAM: Instance = https://{tenant-name}.ciamlogin.com/, Authority = https://{tenant-name}.ciamlogin.com/{tenant-id}/v2.0
// For regular Entra ID: Instance = https://login.microsoftonline.com, Authority = organizations/v2.0 or tenant/v2.0
var instance = string.IsNullOrEmpty(ciamTenantName)
    ? (Environment.GetEnvironmentVariable("AzureAd__Instance") ?? "https://login.microsoftonline.com/")
    : $"https://{ciamTenantName}.ciamlogin.com/";
var authority = string.IsNullOrEmpty(ciamTenantName)
    ? (string.IsNullOrEmpty(tenantId) ? $"{instance.TrimEnd('/')}/organizations/v2.0" : $"{instance.TrimEnd('/')}/{tenantId}/v2.0")
    : $"{instance.TrimEnd('/')}/{tenantId}/v2.0";
var domainForConfig = string.IsNullOrEmpty(ciamTenantName)
    ? domain
    : $"{ciamTenantName}.onmicrosoft.com";

// Map env vars to AzureAd config section for Microsoft.Identity.Web token acquisition
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["AzureAd:Instance"] = instance,
    ["AzureAd:TenantId"] = tenantId,
    ["AzureAd:ClientId"] = clientId,
    ["AzureAd:ClientSecret"] = clientSecret,
    ["AzureAd:CallbackPath"] = "/signin-oidc",
    ["AzureAd:SignedOutCallbackPath"] = "/signout-callback-oidc",
    ["AzureAd:Authority"] = authority,
    ["AzureAd:Domain"] = domainForConfig
});

builder.Services.AddRazorPages();

// Add forwarded headers for Azure App Service (load balancer)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Distributed cache for session token cache (survives restarts/scale-out)
// Requires REDIS_CONNECTION_STRING env var in Azure App Service
var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
if (!string.IsNullOrEmpty(redisConn))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConn;
        options.InstanceName = "RallyingIsLife:";
    });
}
else
{
    // Fallback: in-memory (dev only) - does NOT survive restarts
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(Array.Empty<string>())
    .AddDistributedTokenCaches(); // Uses distributed cache (Redis or memory)

var cookieDomain = Environment.GetEnvironmentVariable("CookieDomain") ?? "";

// Configure cookie authentication to work with custom domain (www.loverallying.uk)
// Uses PostConfigure to run after Microsoft.Identity.Web sets up the cookie scheme
if (!string.IsNullOrEmpty(cookieDomain))
{
    builder.Services.PostConfigure<CookieAuthenticationOptions>(
        CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.Cookie.Domain = cookieDomain; // e.g., ".loverallying.uk"
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
}

builder.Services.AddControllersWithViews(options =>
{
    // Handle consent/conditional access challenges for Razor Pages and Controllers
    options.Filters.Add<ConsentChallengeFilter>();
}).AddMicrosoftIdentityUI();

builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddPolicy("AllowFE", p => p.WithOrigins(Environment.GetEnvironmentVariable("feRallyingIsLifeBaseURL") ?? "https://localhost:3000").AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddAuthorization();

// Session required by AddDistributedTokenCaches() (stores tokens in distributed cache).
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

// Use forwarded headers BEFORE authentication middleware
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// CORS between UseRouting and UseAuthentication (handles preflights on protected endpoints)
app.UseCors("AllowFE");

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();
app.MapLoginAndLogout();

app.MapGet("/health", () => Results.Json(new { status = "healthy", service = "App-FrontEnd" }));

app.Run();

// MVC filter to handle consent challenges (MicrosoftIdentityWebChallengeUserException)
// Redirects to Microsoft Identity UI Account controller for interactive consent
public class ConsentChallengeFilter : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is MicrosoftIdentityWebChallengeUserException)
        {
            // Don't redirect if already in consent flow or callback path
            var path = context.HttpContext.Request.Path.Value ?? "";
            if (path.StartsWith("/MicrosoftIdentity/Account/SignIn", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/signin-oidc", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/signout-callback-oidc", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask; // Let it propagate
            }

            // Redirect to the Microsoft Identity UI Account controller for consent
            var redirectUri = $"/MicrosoftIdentity/Account/SignIn?redirectUri={Uri.EscapeDataString(context.HttpContext.Request.Path + context.HttpContext.Request.QueryString)}";
            context.HttpContext.Response.Redirect(redirectUri);
            context.ExceptionHandled = true;
        }
        return Task.CompletedTask;
    }
}