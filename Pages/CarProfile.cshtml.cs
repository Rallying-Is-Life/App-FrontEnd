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
                        // Find the entrant for this car in this rally
                        // The API doesn't directly link car to entrant, but we can try to match by make/model
                        // For now, we'll leave position/status as null and they'll show as "—"
                        // The API would need to be enhanced to support car->entrant lookup
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