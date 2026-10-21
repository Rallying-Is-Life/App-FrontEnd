using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Azure AD / Entra External ID settings (from web-app settings, never committed)
var instance = Environment.GetEnvironmentVariable("AzureAd__Instance") ?? "https://login.microsoftonline.com";
var tenantId = Environment.GetEnvironmentVariable("AzureAd__TenantId") ?? Environment.GetEnvironmentVariable("Auth__TenantId") ?? "";
var clientId = Environment.GetEnvironmentVariable("AzureAd__ClientId") ?? "";
var clientSecret = Environment.GetEnvironmentVariable("AzureAd__ClientSecret") ?? "";

builder.Services.AddAuthentication("AzureAd")
    .AddMicrosoftIdentityWebApp(options =>
    {
        options.Instance = instance;
        options.TenantId = tenantId;
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";
    })
    .EnableTokenAcquisitionToCallDownstreamApi(new[] { "api://rallyingislife-api-dev/Api.Read" })
    .AddSessionTokenCaches();


builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddPolicy("AllowFE", p => p.WithOrigins(Environment.GetEnvironmentVariable("feRallyingIsLifeBaseURL") ?? "https://localhost:3000").AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCookiePolicy();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowFE");

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();
app.MapLoginAndLogout();

app.Run();
