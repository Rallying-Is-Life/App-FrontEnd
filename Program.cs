using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// Azure AD / Entra External ID settings (from web-app settings, never committed)
var instance = Environment.GetEnvironmentVariable("AzureAd__Instance") ?? "https://login.microsoftonline.com";
var tenantId = Environment.GetEnvironmentVariable("TenantID")
    ?? Environment.GetEnvironmentVariable("AzureAd__TenantId")
    ?? Environment.GetEnvironmentVariable("Auth__TenantId") ?? "";
var clientId = Environment.GetEnvironmentVariable("ClientID")
    ?? Environment.GetEnvironmentVariable("AzureAd__ClientId") ?? "";
var clientSecret = Environment.GetEnvironmentVariable("ClientSecret")
    ?? Environment.GetEnvironmentVariable("AzureAd__ClientSecret") ?? "";
// Downstream API scope for token acquisition (full format: api://<api-client-id>/Scope).
// Fallback uses API client ID from env var if provided, otherwise bare scopes (legacy).
var apiClientId = Environment.GetEnvironmentVariable("ApiClientId") ?? "";
var downstreamScopesRaw = Environment.GetEnvironmentVariable("DownstreamApiScopes")
    ?? Environment.GetEnvironmentVariable("DownstreamApiScope")
    ?? (string.IsNullOrEmpty(apiClientId) ? "Api.Read,Api.Write" : $"api://{apiClientId}/Api.Read,api://{apiClientId}/Api.Write");
var downstreamScopes = downstreamScopesRaw.Split(',').Select(s => s.Trim()).ToArray();

// Map env vars to AzureAd config section for Microsoft.Identity.Web token acquisition
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["AzureAd:Instance"] = "https://login.microsoftonline.com",
    ["AzureAd:TenantId"] = tenantId,
    ["AzureAd:ClientId"] = clientId,
    ["AzureAd:ClientSecret"] = clientSecret,
    ["AzureAd:CallbackPath"] = "/signin-oidc",
    ["AzureAd:SignedOutCallbackPath"] = "/signout-callback-oidc"
});

builder.Services.AddRazorPages();

// Build authority: if tenantId provided, use tenant-specific endpoint; else use organizations (work/school only)
var authority = string.IsNullOrEmpty(tenantId)
    ? $"{instance.TrimEnd('/')}/organizations/v2.0"
    : $"{instance.TrimEnd('/')}/{tenantId}/v2.0";

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
    .EnableTokenAcquisitionToCallDownstreamApi(downstreamScopes)
    .AddDistributedTokenCaches(); // Uses distributed cache (Redis or memory)

// Configure OpenIdConnect options for correlation cookie behind load balancer
builder.Services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = authority;
    options.CallbackPath = "/signin-oidc";
    options.SignedOutCallbackPath = "/signout-callback-oidc";
    // Entra ID returns via cross-site form_post; correlation/nonce cookies must be SameSite=None with Secure=true
    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.CorrelationCookie.IsEssential = true;
    options.NonceCookie.SameSite = SameSiteMode.None;
    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.NonceCookie.IsEssential = true;
});

builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddPolicy("AllowFE", p => p.WithOrigins(Environment.GetEnvironmentVariable("feRallyingIsLifeBaseURL") ?? "https://localhost:3000").AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddAuthorization();

// Session required by AddDistributedTokenCaches() (stores OBO tokens in distributed cache).
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Configure cookie policy for correlation cookie
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.Always;
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

app.UseCookiePolicy();
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