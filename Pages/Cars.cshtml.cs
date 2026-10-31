using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
namespace App_FrontEnd.Pages;
public class CarsModel : PageModel {
    [BindProperty(SupportsGet = true)]
    public new int PageNum { get; set; } = 1;
    public const int MaxPageSize = 100;
    public List<CarItem> Cars { get; set; } = new();
    public async Task OnGetAsync() {
    if (PageNum < 1) PageNum = 1;
    using var client = new HttpClient();
        var baseUrl = Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL") ?? "https://dev-rallyingislife.azurewebsites.net/";
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        var skip = (PageNum - 1) * 20;
        Cars = await client.GetFromJsonAsync<List<CarItem>>("cars?page=" + PageNum + "&pageSize=20") ?? new List<CarItem>();
    }
}
