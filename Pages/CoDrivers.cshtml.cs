using Microsoft.AspNetCore.Mvc.RazorPages;
namespace App_FrontEnd.Pages;
public class CoCoCoDriversModel : PageModel
{
    public List<PersonItem> People { get; set; } = new();
    public async Task OnGetAsync(string q = null)
    {
        var url = "/people?type=co-driver" + (q != null ? $"&q={q}" : "");
        try { People = await new HttpClient().GetFromJsonAsync<List<PersonItem>>(url) ?? new(); } catch { People = new(); }
    }
}
