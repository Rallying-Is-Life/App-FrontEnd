using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
                var data = await c.GetFromJsonAsync<MissingCanHelpResponse>($"{baseUrl}/helpneeded/cars");
                if (data != null)
                {
                    Items = data.Items ?? new();
                    Count = data.Count;
                }
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<CanHelpModel>>();
                logger.LogError(ex, "Failed to fetch CanHelp data from {Url}", baseUrl);
            }
        }
    }
}