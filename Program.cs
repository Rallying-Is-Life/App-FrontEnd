using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
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
// Downstream API (custom API) configuration
var apiClientId = Environment.GetEnvironmentVariable("ApiClientId") ?? "";
var apiBaseUrl = Environment.GetEnvironmentVariable("ApiBaseUrl") ?? "";
var apiScopes = Environment.GetEnvironmentVariable("ApiScopes") ?? 
    (string.IsNullOrEmpty(apiClientId) ? "Api.Read,Api.Write" : $"api://{apiClientId}/Api.Read,api://{apiClientId}/Api.Write");

// Build authority: if tenantId provided, use tenant-specific endpoint; else use organizations (work/school only)
var authority = string.IsNullOrEmpty(tenantId)
    ? $"{instance.TrimEnd('/')}/organizations/v2.0"
    : $"{instance.TrimEnd('/')}/{tenantId}/v2.0";

// Map env vars to AzureAd config section for Microsoft.Identity.Web token acquisition
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["AzureAd:Instance"] = "https://login.microsoftonline.com",
    ["AzureAd:TenantId"] = tenantId,
    ["AzureAd:ClientId"] = clientId,
    ["AzureAd:ClientSecret"] = clientSecret,
    ["AzureAd:CallbackPath"] = "/signin-oidc",
    ["AzureAd:SignedOutCallbackPath"] = "/signout-callback-oidc",
    ["AzureAd:Authority"] = authority
});

// Downstream API config section for Microsoft.Identity.Web
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["DownstreamApi:BaseUrl"] = apiBaseUrl,
    ["DownstreamApi:Scopes"] = apiScopes
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
    .EnableTokenAcquisitionToCallDownstreamApi() // For Microsoft Graph (if needed)
    .AddDownstreamWebApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
    .AddDistributedTokenCaches(); // Uses distributed cache (Redis or memory)

builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

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

// Middleware to handle consent challenges (MicrosoftIdentityWebChallengeUserException)
// Placed AFTER UseAuthorization so it only catches exceptions from page handlers, not auth middleware
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (MicrosoftIdentityWebChallengeUserException)
    {
        // Don't redirect if already in consent flow or callback path
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/MicrosoftIdentity/Account/SignIn", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/signin-oidc", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/signout-callback-oidc", StringComparison.OrdinalIgnoreCase))
        {
            throw; // Let it propagate
        }

        // Redirect to the Microsoft Identity UI Account controller for consent
        var redirectUri = $"/MicrosoftIdentity/Account/SignIn?redirectUri={Uri.EscapeDataString(context.Request.Path + context.Request.QueryString)}";
        context.Response.Redirect(redirectUri);
    }
});

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();
app.MapLoginAndLogout();

app.MapGet("/health", () => Results.Json(new { status = "healthy", service = "App-FrontEnd" }));

app.Run();