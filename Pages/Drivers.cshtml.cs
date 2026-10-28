using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
namespace App_FrontEnd.Pages;
public class DriversModel : PageModel
{
    public List<PersonItem> People { get; set; } = new();
    public async Task OnGetAsync(string q = null)
    {
        using var client = new HttpClient();
        var baseUrl = Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL") ?? "https://dev-rallyingislife-api.azurewebsites.net/";
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        People = await client.GetFromJsonAsync<List<PersonItem>>("people?type=driver" + (q != null ? $"&q={q}" : "")) ?? new();
    }
}
public class PersonItem { public string firstName { get; set; } public string surname { get; set; } public string nationality { get; set; } }
