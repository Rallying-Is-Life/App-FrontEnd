using System.Net.Http;
using System.Net.Http.Json;
using App_FrontEnd.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace App_FrontEnd.Pages;
public class CarsModel : PageModel {
public List<CarItem> Cars { get; set; } = new();
public async Task OnGetAsync() {
using var client = new HttpClient();
var baseUrl = Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL") ?? "https://dev-rallyingislife.azurewebsites.net/";
client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
Cars = await client.GetFromJsonAsync<List<CarItem>>("cars?page=1&pageSize=20") ?? new List<CarItem>();
}
}
