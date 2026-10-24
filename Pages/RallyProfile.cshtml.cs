using Microsoft.AspNetCore.Mvc.RazorPages; using System.Net.Http.Json; using Microsoft.Extensions.Configuration; using System.Linq;
namespace App_FrontEnd.Pages;
public class RallyProfile : PageModel
{
    public string RallyName { get; set; } = string.Empty; public DateTime EventDate { get; set; } public double LengthKm { get; set; } public int StageCount { get; set; } public List<StageInfo> Stages { get; set; } = new();
    public async Task OnGetAsync()
    {
        var id = Request.Query["id"].ToString();
        if (!string.IsNullOrEmpty(id))
        {
            using var c = new HttpClient();
            var baseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["apiRallyingIsLifeBaseURL"]?.TrimEnd('/') ?? "";
            var data = await c.GetFromJsonAsync<RallyProfileItem>($"{baseUrl}/profiles/rally?id={id}");
            if (data != null) { RallyName = data.RallyName; EventDate = data.EventDate; LengthKm = data.LengthKm; StageCount = data.StageCount; Stages = data.Stages ?? new(); }
            if (!Stages.Any() && !string.IsNullOrEmpty(id)) { try { var stages = await c.GetFromJsonAsync<List<StageInfo>>($"{baseUrl}/profiles/rally/stages?id={id}"); if (stages != null) Stages = stages; } catch { } }
        }
    }
}
public class StageInfo { public int StageNumber { get; set; } public string StageName { get; set; } = string.Empty; public double StageLength { get; set; } }
public class RallyProfileItem { public string RallyName { get; set; } = string.Empty; public DateTime EventDate { get; set; } public double LengthKm { get; set; } public int StageCount { get; set; } public string RallyRowKey { get; set; } = string.Empty; public List<StageInfo>? Stages { get; set; } }
