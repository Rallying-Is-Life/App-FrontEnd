using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;
namespace RallyingIsLife.Pages;
[Authorize]
public class ProtectedModel(ITokenAcquisition tokenAcquisition, IConfiguration config) : PageModel
{
    public string? ApiResult { get; set; }
    public string? TenantId { get; set; }

    public async Task OnGetAsync()
    {
        TenantId = config["TenantID"] ?? config["AzureAd__TenantId"];

        try
        {
            // Environment-specific values come from app settings, never hardcoded.
            var apiBaseUrl = config["apiRallyingIsLifeBaseURL"]
                ?? throw new InvalidOperationException("apiRallyingIsLifeBaseURL is not configured.");
            var apiUrl = apiBaseUrl.TrimEnd('/') + "/stats";

            var configuredScopes = config["DownstreamApiScopes"] ?? config["DownstreamApiScope"] ?? "";
            var scopes = configuredScopes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (scopes.Length == 0)
                throw new InvalidOperationException("DownstreamApiScopes is not configured.");

            var token = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await client.GetAsync(apiUrl);
            ApiResult = $"{(int)resp.StatusCode} {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}";
        }
        catch (MsalUiRequiredException)
        {
            // Trigger interactive authorization to populate session token cache (incremental consent / CA).
            HttpContext.Response.Redirect("/login?returnUrl=" + System.Net.WebUtility.UrlEncode("/Protected"));
            return;
        }
        catch (Exception ex)
        {
            ApiResult = "OBO error: " + ex.Message;
        }
    }
}
