using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;
namespace RallyingIsLife.Pages;
[Authorize]
public class ProtectedModel(ITokenAcquisition tokenAcquisition, IConfiguration config) : PageModel
{
    public string? ApiResult { get; set; }
    public async Task OnGetAsync()
    {
        try
        {
            var apiUrl = (config["apiRallyingIsLifeBaseURL"] ?? "https://dev-rallyingislife-api.azurewebsites.net") + "/stats";
            var token = await tokenAcquisition.GetAccessTokenForUserAsync(new[] { "api://rallyingislife-api-dev/Api.Read" });
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await client.GetAsync(apiUrl);
            ApiResult = $"{(int)resp.StatusCode} {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}";
        }
        catch (Exception ex)
        {
            ApiResult = "OBO error: " + ex.Message;
        }
    }
}
