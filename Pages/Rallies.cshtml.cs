using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;
namespace RallyingIsLife.Pages { public class RallyItem { public int Year {get;set;} public string EventName {get;set;} public double CompetitiveDistanceKm {get;set;} public string DistanceUnit {get;set;} } public class RalliesModel : PageModel { public System.Collections.Generic.List<RallyItem> Rallies {get;set;}=new(); public async Task OnGetAsync() { using var c=new HttpClient(); var b=Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL")??"https://dev-rallyingislife.azurewebsites.net/"; c.BaseAddress=new Uri(b.TrimEnd('/')+"/"); Rallies=await c.GetFromJsonAsync<System.Collections.Generic.List<RallyItem>>("rallies?page=1&size=20")??new(); } } }
