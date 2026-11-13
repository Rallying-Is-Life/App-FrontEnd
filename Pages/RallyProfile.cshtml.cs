using Microsoft.AspNetCore.Mvc.RazorPages; 
using System.Net.Http.Json; 
using Microsoft.Extensions.Configuration; 
using System.Linq;

namespace App_FrontEnd.Pages;
public class RallyProfile : PageModel
{
    public string RallyName { get; set; } = string.Empty; 
    public DateTime EventDate { get; set; } 
    public DateTime? EndDate { get; set; } 
    public double LengthKm { get; set; } 
    public int StageCount { get; set; } 
    public int EntryCount { get; set; } 
    public int FinishCount { get; set; }
    public int RetireCount { get; set; }
    public string Website { get; set; } = string.Empty; 
    public List<StageInfo> Stages { get; set; } = new(); 
    public string? Country { get; set; } 
    public string? Region { get; set; } 
    public string? FlagUrl { get; set; } 
    public string? LogoUrl { get; set; } 
    public string RallyRowKey { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var id = Request.Query["id"].ToString();
        if (!string.IsNullOrEmpty(id))
        {
            using var c = new HttpClient();
            var baseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["apiRallyingIsLifeBaseURL"]?.TrimEnd('/') ?? "";
            var data = await c.GetFromJsonAsync<RallyProfileItem>($"{baseUrl}/profiles/rally?id={id}");
            if (data != null) 
            { 
                RallyName = data.RallyName; 
                EventDate = data.EventDate; 
                EndDate = data.EndDate; 
                LogoUrl = data.LogoUrl; 
                LengthKm = data.LengthKm; 
                StageCount = data.StageCount; 
                EntryCount = data.EntryCount; 
                FinishCount = data.FinishCount;
                RetireCount = data.RetireCount;
                Website = data.Website ?? ""; 
                Stages = data.Stages ?? new(); 
                Country = data.Country; 
                Region = data.Region; 
                FlagUrl = data.FlagUrl; 
                RallyRowKey = data.RallyRowKey; 
            }
            if (!Stages.Any() && !string.IsNullOrEmpty(id)) 
            { 
                try 
                { 
                    var stages = await c.GetFromJsonAsync<List<StageInfo>>($"{baseUrl}/profiles/rally/stages?id={id}"); 
                    if (stages != null) Stages = stages; 
                } 
                catch { } 
            }

            // Fetch finish/retire counts from leaderboard endpoints
            try
            {
                var lbData = await c.GetFromJsonAsync<LeaderboardResponse>($"{baseUrl}/leaderboard?id={id}");
                if (lbData != null)
                {
                    FinishCount = lbData.Items?.Count(x => string.Equals(x.Status, "Finished", StringComparison.OrdinalIgnoreCase)) ?? 0;
                }
            }
            catch { }

            try
            {
                var retData = await c.GetFromJsonAsync<RetirementsResponse>($"{baseUrl}/leaderboard/retirements?id={id}");
                if (retData != null)
                {
                    RetireCount = retData.Count;
                }
            }
            catch { }
        }
    }
}