using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
namespace App_FrontEnd.Pages;
public class CarProfile : PageModel {
public string Make { get; set; } = string.Empty;
public string Model { get; set; } = string.Empty;
public string VRM { get; set; } = string.Empty;
public List<CarRally> Rallies { get; set; } = new();
public async Task OnGetAsync() {
var id = Request.Query["id"].ToString();
var baseUrl = (HttpContext.RequestServices.GetRequiredService<IConfiguration>()["apiRallyingIsLifeBaseURL"] ?? "").TrimEnd('/');
using var c = new HttpClient();
var data = await c.GetFromJsonAsync<CarProfileApiResponse>($"{baseUrl}/profiles/car?id={id}");
if (data != null) { Make = data.make; Model = data.model; VRM = data.vrm; Rallies = data.rallies ?? new(); }
}
public class CarRally { public string rallyName { get; set; } = ""; public int year { get; set; } public string rallyRowKey { get; set; } = ""; }
public class CarProfileApiResponse { public string make { get; set; } = ""; public string model { get; set; } = ""; public string vrm { get; set; } = ""; public string rowKey { get; set; } = ""; public List<CarRally>? rallies { get; set; } }
}