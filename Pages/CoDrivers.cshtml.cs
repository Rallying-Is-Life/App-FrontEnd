using Microsoft.AspNetCore.Mvc.RazorPages;
namespace App_FrontEnd.Pages;
public class CoCoCoDriversModel : PageModel
{
    public List<PersonItem> People { get; set; } = new();
    public async Task OnGetAsync(string q = null)
    {
        var url = (Configuration["apiRallyingIsLifeBaseURL"] ?? "/stats").TrimEnd('/') + "/people?type=co-driver" + (q != null ? $"&q={q}" : "");
        try { People = await new HttpClient().GetFromJsonAsync<List<PersonItem>>(url) ?? new(); } catch { People = new(); }
    }
}
public class PersonItem { public string firstName { get; set; } public string surname { get; set; } public string nationality { get; set; } public string eventName { get; set; } public string eventYear { get; set; } }
