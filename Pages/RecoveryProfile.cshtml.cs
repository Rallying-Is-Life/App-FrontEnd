using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App_FrontEnd.Pages;

public class RecoveryProfile : PageModel
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public string? Name { get; set; }
    public async Task OnGetAsync()
    {
        var id = Request.Query["id"].ToString();
        RowKey = id;
        Name = id ?? "Unknown";
    }
}
