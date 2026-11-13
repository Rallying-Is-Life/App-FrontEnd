using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace App_FrontEnd.Pages
{
    public class EntryListUnseeded : PageModel
    {
        public string RallyRowKey { get; set; } = string.Empty;
        public string RallyName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public List<EntryItem> Entries { get; set; } = new();
        public int EntryCount { get; set; }

        public async Task OnGetAsync()
        {
            RallyRowKey = Request.Query["id"].ToString();
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

            // Get unseeded entry list
            try
            {
                var data = await c.GetFromJsonAsync<EntryListResponse>($"{baseUrl}/entries/unseeded?id={RallyRowKey}");
                if (data != null)
                {
                    Entries = data.Items ?? new();
                    EntryCount = data.Count;
                }
            }
            catch { }
        }
    }
}
