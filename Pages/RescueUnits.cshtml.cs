using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using App_FrontEnd.Models;
namespace App_FrontEnd.Pages;
public class RescueUnitsModel : PageModel
{
    public List<UnitItem> Units { get; set; } = new();
    public async Task OnGetAsync(int page = 1)
    {
        using var client = new HttpClient();
        var baseUrl = Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL") ?? "https://dev-rallyingislife.azurewebsites.net/";
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        Units = await client.GetFromJsonAsync<List<UnitItem>>("rescue?page=" + page) ?? new List<UnitItem>();
    }
}
