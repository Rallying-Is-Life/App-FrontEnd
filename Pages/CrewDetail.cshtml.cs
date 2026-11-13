using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace App_FrontEnd.Pages
{
    public class CrewDetail : PageModel
    {
        public string RallyRowKey { get; set; } = string.Empty;
        public string EntrantId { get; set; } = string.Empty;
        public string RallyName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string CoDriverName { get; set; } = string.Empty;
        public string RaceNumber { get; set; } = string.Empty;
        public string RaceClass { get; set; } = string.Empty;
        public List<StageTimeDetail> Stages { get; set; } = new();
        public int? TotalTimeSeconds { get; set; }
        public int StagesCompleted { get; set; }
        public int TotalStages { get; set; }
        public string Status { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            RallyRowKey = Request.Query["rallyRowKey"].ToString();
            EntrantId = Request.Query["entrantId"].ToString();

            if (string.IsNullOrEmpty(RallyRowKey) || string.IsNullOrEmpty(EntrantId))
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

            // Get crew detail data
            try
            {
                var data = await c.GetFromJsonAsync<CrewDetailResponse>($"{baseUrl}/leaderboard/entrant?id={RallyRowKey}&entrantId={EntrantId}");
                if (data != null)
                {
                    Stages = data.Stages ?? new();
                    TotalTimeSeconds = data.TotalTimeSeconds;
                    StagesCompleted = data.StagesCompleted;
                    TotalStages = data.TotalStages;
                    Status = data.Status;
                    
                    // Get driver/co-driver names from first stage or we could call leaderboard for the specific entrant
                    // The API response doesn't include driver/co-driver names, so we need to get them
                    // We'll fetch from leaderboard to get those details
                    try
                    {
                        var lbData = await c.GetFromJsonAsync<LeaderboardResponse>($"{baseUrl}/leaderboard?id={RallyRowKey}");
                        if (lbData != null)
                        {
                            var entrant = lbData.Items.FirstOrDefault(x => string.Equals(x.EntrantId, EntrantId, StringComparison.OrdinalIgnoreCase));
                            if (entrant != null)
                            {
                                DriverName = entrant.DriverName;
                                CoDriverName = entrant.CoDriverName;
                                RaceNumber = entrant.RaceNumber;
                                RaceClass = entrant.RaceClass;
                            }
                        }
                    }
                    catch { }
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

        public string FormatStageTime(int? seconds)
        {
            if (!seconds.HasValue) return "—";
            var ts = TimeSpan.FromSeconds(seconds.Value);
            if (ts.TotalHours >= 1)
                return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        public string GetStatusBadgeClass(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "finished" => "bg-success",
                "retired" => "bg-danger",
                "running" => "bg-primary",
                "superrally" => "bg-info text-dark",
                "excluded" => "bg-dark",
                "dns" => "bg-secondary",
                "dnf" => "bg-danger",
                "dsq" => "bg-dark",
                _ => "bg-secondary"
            };
        }
    }
}