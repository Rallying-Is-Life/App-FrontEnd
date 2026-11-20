using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace App_FrontEnd.Pages
{
    public class StageProfile : PageModel
    {
        public string RallyRowKey { get; set; } = string.Empty;
        public int StageSeq { get; set; }
        public string RallyName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public StageInfo Stage { get; set; } = new();
        public List<StageUnitLink> RescueUnits { get; set; } = new();
        public List<StageUnitLink> RecoveryUnits { get; set; } = new();
        public List<StageLeaderboardItem> Leaderboard { get; set; } = new();

        public async Task OnGetAsync()
        {
            RallyRowKey = Request.Query["id"].ToString();
            var stageSeqStr = Request.Query["stageSeq"].ToString();

            if (string.IsNullOrEmpty(RallyRowKey) || !int.TryParse(stageSeqStr, out var seq))
                return;

            StageSeq = seq;

            using var c = new HttpClient();
            var baseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["apiRallyingIsLifeBaseURL"]?.TrimEnd('/') ?? "";

            if (string.IsNullOrEmpty(baseUrl))
                return;

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

            // Get stage profile (details + rescue/recovery + leaderboard)
            try
            {
                var data = await c.GetFromJsonAsync<StageProfileResponse>($"{baseUrl}/profiles/stage?id={RallyRowKey}&stageSeq={StageSeq}");
                if (data != null)
                {
                    Stage = data.Stage ?? new();
                    RescueUnits = data.RescueUnits ?? new();
                    RecoveryUnits = data.RecoveryUnits ?? new();
                    Leaderboard = data.Leaderboard ?? new();
                }
            }
            catch { }
        }

        public string FormatTime(double? totalSeconds)
        {
            if (!totalSeconds.HasValue) return "—";
            var ts = TimeSpan.FromSeconds(totalSeconds.Value);
            if (ts.TotalHours >= 1)
                return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3}".TrimEnd('0').TrimEnd('.');
            return $"{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3}".TrimEnd('0').TrimEnd('.');
        }

        public string GetStatusBadgeClass(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "finished" => "bg-success",
                "retired" => "bg-danger",
                "running" => "bg-info",
                "superrally" => "bg-info text-dark",
                "excluded" => "bg-dark",
                "dns" => "bg-secondary",
                "dnf" => "bg-danger",
                "dsq" => "bg-dark",
                _ => "bg-secondary"
            };
        }

        public string GetTrophyEmoji(int position)
        {
            return position switch
            {
                1 => " 🥇",
                2 => " 🥈",
                3 => " 🥉",
                _ => ""
            };
        }
    }

    // Response models matching API
    public class StageProfileResponse
    {
        public StageInfo Stage { get; set; } = new();
        public List<StageUnitLink> RescueUnits { get; set; } = new();
        public List<StageUnitLink> RecoveryUnits { get; set; } = new();
        public List<StageLeaderboardItem> Leaderboard { get; set; } = new();
    }

    public class StageUnitLink
    {
        public string UnitId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string PointType { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
    }

    public class StageLeaderboardItem
    {
        public int OverallPosition { get; set; }
        public int ClassPosition { get; set; }
        public string EntrantId { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string CoDriverName { get; set; } = string.Empty;
        public string RaceNumber { get; set; } = string.Empty;
        public string RaceClass { get; set; } = string.Empty;
        public double? TimeSeconds { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}