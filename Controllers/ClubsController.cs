using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using App_FrontEnd.Models;

namespace App_FrontEnd.Controllers;
public class ClubsController : Controller
{
    [HttpGet("/Clubs")]
    public async Task<IActionResult> Index()
    {
        using var client = new HttpClient();
        var baseUrl = Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL") ?? "https://dev-rallyingislife.azurewebsites.net/";
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        var result = await client.GetFromJsonAsync<List<ClubItem>>("clubs?page=1&pageSize=20");
        return View(result ?? new List<ClubItem>());
    }
}
