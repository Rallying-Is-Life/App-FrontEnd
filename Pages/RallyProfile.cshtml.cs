using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;

namespace App_FrontEnd.Pages;

public class RallyProfile : PageModel
{
    public string RallyName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public double LengthKm { get; set; }
    public int StageCount { get; set; }
    public List<StageInfo> Stages { get; set; } = new();

    public async Task OnGetAsync()
    {
        var id = Request.Query["id"].ToString();
        if (!string.IsNullOrEmpty(id))
        {
            using var client = new HttpClient();
            var baseUrl = Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL") ?? "https://localhost:5000/";
            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");

            var profiles = await client.GetFromJsonAsync<List<RallyProfileItem>>("profiles/rally") ?? new();
            var profile = profiles.FirstOrDefault(p => p.RallyRowKey == id);
            if (profile != null)
            {
                RallyName = profile.RallyName;
                EventDate = profile.EventDate;
                LengthKm = profile.LengthKm;
                StageCount = profile.StageCount;
            }
        }
    }
}

public class StageInfo
{
    public int StageNumber { get; set; }
    public string StageName { get; set; } = string.Empty;
    public double StageLength { get; set; }
}

public class RallyProfileItem
{
    public string RallyName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public double LengthKm { get; set; }
    public int StageCount { get; set; }
    public string RallyRowKey { get; set; } = string.Empty;
}
