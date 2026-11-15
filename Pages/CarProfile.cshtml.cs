using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;

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
        if (data != null) { 
            Make = data.make; 
            Model = data.model; 
            VRM = data.vrm; 
            Rallies = data.rallies ?? new(); 
        }

        // For each rally, fetch leaderboard to get position/status for this car
        if (Rallies.Any())
        {
            foreach (var rally in Rallies)
            {
                try
                {
                    var lbData = await c.GetFromJsonAsync<LeaderboardResponse>($"{baseUrl}/leaderboard?id={rally.rallyRowKey}");
                    if (lbData != null)
                    {
                        var entrant = lbData.Items.FirstOrDefault(x =>
                            string.Equals(x.EntrantId, rally.EntrantId, StringComparison.OrdinalIgnoreCase));
                        if (entrant != null)
                        {
                            rally.Status = entrant.Status;
                            rally.OverallPosition = entrant.OverallPosition;
                            rally.ClassPosition = entrant.ClassPosition;
                        }
                    }
                }
                catch { }
            }
        }
    }

    public class CarRally { 
        public string rallyName { get; set; } = ""; 
        public int year { get; set; } 
        public string rallyRowKey { get; set; } = ""; 
        public string? EntrantId { get; set; }
        public string? Status { get; set; }
        public int? OverallPosition { get; set; }
        public int? ClassPosition { get; set; }
    }
    public class CarProfileApiResponse { 
        public string make { get; set; } = ""; 
        public string model { get; set; } = ""; 
        public string vrm { get; set; } = ""; 
        public string rowKey { get; set; } = ""; 
        public List<CarRally>? rallies { get; set; } 
    }
}