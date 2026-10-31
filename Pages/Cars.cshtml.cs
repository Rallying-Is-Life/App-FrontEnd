using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
namespace App_FrontEnd.Pages;
public class CarsModel : PageModel {
    public List<CarItem> Cars { get; set; } = new();
    public int Page { get; set; } = 1;
    [BindProperty(SupportsGet = true, Name = "p")]
    public int PageParam { get; set; } = 1;
    public async Task OnGetAsync() {
        int qPage = 1;
        int.TryParse(Request.Query["p"], out qPage);
        Page = (PageParam > 0 ? PageParam : (qPage > 0 ? qPage : 1));
        if (Page < 1) Page = 1;
        using var client = new HttpClient();
        var baseUrl = Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL") ?? "https://dev-rallyingislife.azurewebsites.net/";
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        var url = "cars?page=" + Page + "&pageSize=20";
if (!string.IsNullOrWhiteSpace(Request.Query["q"])) url += "&q=" + Uri.EscapeDataString(Request.Query["q"]);
Cars = await client.GetFromJsonAsync<List<CarItem>>(url) ?? new List<CarItem>();
    }
}
