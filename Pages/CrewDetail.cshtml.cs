using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

        private readonly ILogger<CrewDetail> _logger;

        public CrewDetail(ILogger<CrewDetail> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            RallyRowKey = Request.Query["rallyRowKey"].ToString();
            EntrantId = Request.Query["entrantId"].ToString();

            if (string.IsNullOrEmpty(RallyRowKey) || string.IsNullOrEmpty(EntrantId))
                return;

            using var c = new HttpClient();
            var baseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["apiRallyingIsLifeBaseURL"]?.TrimEnd('/') ?? "";
            _logger.LogInformation("CrewDetail baseUrl={BaseUrl} rallyRowKey={RallyRowKey} entrantId={EntrantId}", baseUrl, RallyRowKey, EntrantId);

            if (string.IsNullOrEmpty(baseUrl))
            {
                _logger.LogError("apiRallyingIsLifeBaseURL config is missing or empty");
                return;
            }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch rally profile for {RallyRowKey}", RallyRowKey);
            }

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
                    _logger.LogInformation("CrewDetail entrant data loaded: stages={Count} totalTime={TotalTime} status={Status}", Stages.Count, TotalTimeSeconds, Status);

                    // Get driver/co-driver names from leaderboard
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
                                _logger.LogInformation("CrewDetail driver info loaded: {Driver} / {CoDriver} #{Number} Class {Class}", DriverName, CoDriverName, RaceNumber, RaceClass);
                            }
                            else
                            {
                                _logger.LogWarning("Entrant {EntrantId} not found in leaderboard for {RallyRowKey}", EntrantId, RallyRowKey);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to fetch leaderboard for driver info: {RallyRowKey}", RallyRowKey);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch crew detail for {RallyRowKey}/{EntrantId}", RallyRowKey, EntrantId);
            }
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