using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using App_FrontEnd.Models;

namespace App_FrontEnd.Pages;
public class ClubsModel : PageModel
{
    public List<ClubItem> Clubs { get; set; } = new();
    public async Task OnGetAsync()
    {
        using var client = new HttpClient();
        var baseUrl = Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL") ?? "https://dev-rallyingislife.azurewebsites.net/";
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        Clubs = await client.GetFromJsonAsync<List<ClubItem>>("clubs?page=1&pageSize=20") ?? new List<ClubItem>();
    }
}
