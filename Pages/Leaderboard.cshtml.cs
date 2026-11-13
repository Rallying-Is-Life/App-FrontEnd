using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace App_FrontEnd.Pages
{
    public class Leaderboard : PageModel
    {
        public string RallyRowKey { get; set; } = string.Empty;
        public string RallyName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? ClassId { get; set; }
        public List<LeaderboardItem> Entries { get; set; } = new();
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

            // Get leaderboard data
            try
            {
                var url = $"{baseUrl}/leaderboard?id={RallyRowKey}";
                if (!string.IsNullOrEmpty(ClassId))
                    url += $"&classId={ClassId}";
                    
                var data = await c.GetFromJsonAsync<LeaderboardResponse>(url);
                if (data != null)
                {
                    Entries = data.Items ?? new();
                    TotalCount = data.Count;
                }
            }
            catch { }
        }

        public string FormatTime(int? totalSeconds)
        {
            if (!totalSeconds.HasValue) return "—";
            var ts = TimeSpan.FromSeconds(totalSeconds.Value);
            return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        public string FormatGap(int? gapSeconds)
        {
            if (!gapSeconds.HasValue) return "—";
            var ts = TimeSpan.FromSeconds(gapSeconds.Value);
            if (ts.TotalHours >= 1)
                return $"+{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            return $"+{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        public string FormatPosition(int? position, List<LeaderboardItem> entries)
        {
            if (!position.HasValue) return "—";
            var pos = position.Value;
            // A position shared by more than one entry is a tie — show "=3" style
            var count = entries.Count(e => e.OverallPosition == pos);
            return count > 1 ? $"={pos}" : pos.ToString();
        }

        public string FormatClassPosition(int? classPosition, string raceClass, List<LeaderboardItem> entries)
        {
            if (!classPosition.HasValue) return "—";
            var pos = classPosition.Value;
            // Tie within this class: same class position shared by more than one entry of the same class
            var count = entries.Count(e =>
                string.Equals(e.RaceClass, raceClass, StringComparison.OrdinalIgnoreCase) &&
                e.ClassPosition == pos);
            return count > 1 ? $"={pos}" : pos.ToString();
        }
    }
}