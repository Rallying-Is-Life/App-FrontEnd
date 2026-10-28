using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
namespace App_FrontEnd.Pages;
public class CoDriversModel : PageModel
{
    public List<PersonItem> People { get; set; } = new();
    public async Task OnGetAsync(string q = null)
    {
        using var client = new HttpClient();
        var baseUrl = Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL") ?? "https://dev-rallyingislife-api.azurewebsites.net/";
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        var resp = await client.GetFromJsonAsync<PeopleResp>("people?type=co-driver" + (q != null ? $"&q={q}" : "")) ?? new();
    }
}

public class PeopleResp { public List<PersonItem> items { get; set; } }
