using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace App_FrontEnd.Pages
{
    public class StageClassification : PageModel
    {
        public string RallyRowKey { get; set; } = string.Empty;
        public string RallyName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public List<StageSelectorItem> StageList { get; set; } = new();
        public int? SelectedStageSeq { get; set; }
        public string? SelectedStageName { get; set; }
        public string? ClassId { get; set; }
        public List<StageClassificationItem> Entries { get; set; } = new();

        public async Task OnGetAsync()
        {
            RallyRowKey = Request.Query["id"].ToString();
            var stageSeqStr = Request.Query["stageSeq"].ToString();
            ClassId = Request.Query["classId"].ToString();

            if (string.IsNullOrEmpty(RallyRowKey))
                return;

            if (int.TryParse(stageSeqStr, out var seq))
                SelectedStageSeq = seq;

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

            // Get stage list for dropdown
            try
            {
                var stages = await c.GetFromJsonAsync<List<StageInfo>>($"{baseUrl}/profiles/rally/stages?id={RallyRowKey}");
                if (stages != null)
                {
                    StageList = stages.Select(s => new StageSelectorItem 
                    { 
                        StageNumber = s.StageNumber, 
                        StageName = s.StageName 
                    }).ToList();
                }
            }
            catch { }

            // Get stage classification if stage selected
            if (SelectedStageSeq.HasValue)
            {
                try
                {
                    var url = $"{baseUrl}/leaderboard/stage?id={RallyRowKey}&stageSeq={SelectedStageSeq.Value}";
                    if (!string.IsNullOrEmpty(ClassId))
                        url += $"&classId={ClassId}";

                    var data = await c.GetFromJsonAsync<StageClassificationResponse>(url);
                    if (data != null)
                    {
                        Entries = data.Items ?? new();
                        SelectedStageName = data.StageName;
                    }
                }
                catch { }
            }
        }

        public string FormatTime(int? seconds)
        {
            if (!seconds.HasValue) return "—";
            var ts = TimeSpan.FromSeconds(seconds.Value);
            if (ts.TotalHours >= 1)
                return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }
}