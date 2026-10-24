using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
namespace App_FrontEnd.Pages;
public class ClubProfile : PageModel
{
    public string ClubName { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public List<ClubRally> Rallies { get; set; } = new();
    public async Task OnGetAsync()
    {
        var id = Request.Query["id"].ToString();
        var baseUrl = (Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL") ?? "https://dev-rallyingislife-api.azurewebsites.net/").TrimEnd('/');
        using var c = new HttpClient();
        var data = await c.GetFromJsonAsync<ClubProfileApiResponse>($"{baseUrl}/profiles/club?id={id}");
        if (data != null) { ClubName = data.clubName; Website = data.website; LogoUrl = data.logoUrl; Rallies = data.rallies ?? new(); }
    }
    public class ClubRally { public string rallyName { get; set; } = ""; public int year { get; set; } }
    public class ClubProfileApiResponse { public string clubName { get; set; } = ""; public string? website { get; set; } public string? logoUrl { get; set; } public List<ClubRally>? rallies { get; set; } }
}
