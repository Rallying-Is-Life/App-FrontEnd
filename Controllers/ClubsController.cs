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
        var apiList = await client.GetFromJsonAsync<List<dynamic>>("clubs?page=1&pageSize=20");
        var result = new List<ClubItem>();
        if (apiList != null)
        {
            foreach (var item in apiList)
            {
                result.Add(new ClubItem
                {
                    ClubName = item?.ClubName?.ToString() ?? "",
                    LogoUrl = item?.LogoUrl?.ToString()
                });
            }
        }
        return View(result);
    }
}
