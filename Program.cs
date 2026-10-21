using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Azure AD / Entra External ID settings (from web-app settings, never committed)
var instance = Environment.GetEnvironmentVariable("AzureAd__Instance") ?? "https://login.microsoftonline.com";
var tenantId = Environment.GetEnvironmentVariable("TenantID")
    ?? Environment.GetEnvironmentVariable("AzureAd__TenantId")
    ?? Environment.GetEnvironmentVariable("Auth__TenantId") ?? "";
var clientId = Environment.GetEnvironmentVariable("ClientID")
    ?? Environment.GetEnvironmentVariable("AzureAd__ClientId") ?? "";
var clientSecret = Environment.GetEnvironmentVariable("ClientSecret")
    ?? Environment.GetEnvironmentVariable("AzureAd__ClientSecret") ?? "";
// Downstream API scope for OBO token acquisition (e.g. "Api.Read,Api.Write").
// Split on ',' so multiple scopes may be configured in one env var.
var downstreamScopesRaw = Environment.GetEnvironmentVariable("DownstreamApiScopes")
    ?? Environment.GetEnvironmentVariable("DownstreamApiScope")
    ?? "Api.Read,Api.Write";
var downstreamScopes = downstreamScopesRaw.Split(',').Select(s => s.Trim()).ToArray();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
.AddMicrosoftIdentityWebApp(options =>
    {
        options.Instance = instance;
        options.TenantId = tenantId;
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";
    })
    .EnableTokenAcquisitionToCallDownstreamApi(downstreamScopes)
    .AddSessionTokenCaches();


builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddPolicy("AllowFE", p => p.WithOrigins(Environment.GetEnvironmentVariable("feRallyingIsLifeBaseURL") ?? "https://localhost:3000").AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddAuthorization();

// Session required by AddSessionTokenCaches() (stores OBO tokens in session).
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Unspecified;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCookiePolicy();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowFE");

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();
app.MapLoginAndLogout();

app.MapGet("/health", () => Results.Json(new { status = "healthy", service = "App-FrontEnd" }));

app.Run();