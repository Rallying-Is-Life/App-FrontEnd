using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace App_FrontEnd.Pages
{
    public class CanHelpModel : PageModel
    {
        public List<MissingCanHelpItem> Items { get; set; } = new();
        public int Count { get; set; }

        public async Task OnGetAsync()
        {
            using var c = new HttpClient();
            var baseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["apiRallyingIsLifeBaseURL"]?.TrimEnd('/') ?? "";
            
            try
            {
                var data = await c.GetFromJsonAsync<MissingCanHelpResponse>($"{baseUrl}/entries/can-help");
                if (data != null)
                {
                    Items = data.Items ?? new();
                    Count = data.Count;
                }
            }
            catch { }
        }
    }
}
