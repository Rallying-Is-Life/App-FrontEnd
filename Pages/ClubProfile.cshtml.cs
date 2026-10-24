using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
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
        var baseUrl = (HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()["apiRallyingIsLifeBaseURL"] ?? "").TrimEnd('/');
        using var c = new HttpClient();
        var data = await c.GetFromJsonAsync<ClubProfileApiResponse>($"{baseUrl}/profiles/club?id={id}");
        if (data != null) { ClubName = data.clubName; Website = data.website; LogoUrl = data.logoUrl; Rallies = data.rallies ?? new(); }
    }
    public class ClubRally { public string rallyName { get; set; } = ""; public int year { get; set; } public string rallyRowKey { get; set; } = ""; }
    public class ClubProfileApiResponse { public string clubName { get; set; } = ""; public string? website { get; set; } public string? logoUrl { get; set; } public List<ClubRally>? rallies { get; set; } }
}
