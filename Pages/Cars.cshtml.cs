using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
namespace App_FrontEnd.Pages;
public class CarsModel : PageModel {
    [BindProperty(SupportsGet = true)]
    public new int Page { get; set; } = 1;
    public const int MaxPageSize = 100;
    public List<CarItem> Cars { get; set; } = new();
    public async Task OnGetAsync(int page = 1) {
        Page = page > 1 ? page : 1;
        if (Page < 1) Page = 1;
        using var client = new HttpClient();
        var baseUrl = Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL") ?? "https://dev-rallyingislife.azurewebsites.net/";
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        var skip = (Page - 1) * 20;
        Cars = await client.GetFromJsonAsync<List<CarItem>>("cars?page=" + Page + "&pageSize=20") ?? new List<CarItem>();
    }
}
