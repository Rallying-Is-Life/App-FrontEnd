using Microsoft.AspNetCore.Mvc.RazorPages;
namespace App_FrontEnd.Pages;
public class CoDriversModel : PageModel
{
    public List<PersonItem> People { get; set; } = new();
    public async Task OnGetAsync(string q = null)
    {
        var url = "/people?type=co-driver" + (q != null ? $"&q={q}" : "");
        try
        {
            var resp = await new HttpClient().GetFromJsonAsync<PeopleResp>(url);
            People = resp?.items ?? new();
        }
        catch { People = new(); }
    }
}
