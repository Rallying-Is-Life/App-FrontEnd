using Microsoft.AspNetCore.Mvc.RazorPages;
namespace App_FrontEnd.Pages;
public class DriversModel : PageModel
{
    public List<PersonItem> People { get; set; } = new();
    public async Task OnGetAsync(string q = null)
    {
        var url = "/people?type=driver" + (q != null ? $"&q={q}" : "");
        try
        {
            var resp = await new HttpClient().GetFromJsonAsync<PeopleResp>(url);
            People = resp?.items ?? new();
        }
        catch { People = new(); }
    }
}
public class PeopleResp { public List<PersonItem> items { get; set; } }
public class PersonItem { public string firstName { get; set; } public string surname { get; set; } public string nationality { get; set; } public string eventName { get; set; } public string eventYear { get; set; } }
