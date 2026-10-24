using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App_FrontEnd.Pages;

public class ClubProfile : PageModel
{
    public string ClubName { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public async Task OnGetAsync()
    {
        var id = Request.Query["id"].ToString();
        ClubName = id ?? "Unknown";
        // Would fetch from API endpoint in a full implementation
    }
}
