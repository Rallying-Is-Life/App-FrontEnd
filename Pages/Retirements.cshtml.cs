using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace App_FrontEnd.Pages
{
    public class Retirements : PageModel
    {
        public string RallyRowKey { get; set; } = string.Empty;
        public string RallyName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? ClassId { get; set; }
        public List<RetirementItem> Entries { get; set; } = new();
        public int TotalCount { get; set; }

        public async Task OnGetAsync()
        {
            RallyRowKey = Request.Query["id"].ToString();
            ClassId = Request.Query["classId"].ToString();

            if (string.IsNullOrEmpty(RallyRowKey))
                return;

            using var c = new HttpClient();
            var baseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["apiRallyingIsLifeBaseURL"]?.TrimEnd('/') ?? "";

            // Get rally profile for name/date
            try
            {
                var rally = await c.GetFromJsonAsync<RallyProfileItem>($"{baseUrl}/profiles/rally?id={RallyRowKey}");
                if (rally != null)
                {
                    RallyName = rally.RallyName;
                    EventDate = rally.EventDate;
                }
            }
            catch { }

            // Get retirements data
            try
            {
                var url = $"{baseUrl}/leaderboard/retirements?id={RallyRowKey}";
                if (!string.IsNullOrEmpty(ClassId))
                    url += $"&classId={ClassId}";

                var data = await c.GetFromJsonAsync<RetirementsResponse>(url);
                if (data != null)
                {
                    Entries = data.Items ?? new();
                    TotalCount = data.Count;
                }
            }
            catch { }
        }
    }
}