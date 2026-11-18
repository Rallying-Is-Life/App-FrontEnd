using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;

namespace App_FrontEnd.Pages;
public class PersonProfile : PageModel
{
    public string FirstName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string FlagUrl { get; set; } = string.Empty;
    public List<PersonEvent> Events { get; set; } = new();

    public async Task OnGetAsync()
    {
        var id = Request.Query["id"].ToString();
        var baseUrl = (HttpContext.RequestServices.GetRequiredService<IConfiguration>()["apiRallyingIsLifeBaseURL"] ?? "").TrimEnd('/');
        using var c = new HttpClient();
        var data = await c.GetFromJsonAsync<PersonProfileApiResponse>($"{baseUrl}/profiles/person?id={id}");
        if (data != null)
        {
            FirstName = data.firstName;
            Surname = data.surname;
            Nationality = data.nationality;
            FlagUrl = data.flagUrl;
            Events = data.events ?? new();
        }
    }

    public class PersonEvent
    {
        public int year { get; set; }
        public string rallyName { get; set; } = "";
        public string rallyRowKey { get; set; } = "";
        public string role { get; set; } = "";
        public string partnerName { get; set; } = "";
        public string carMake { get; set; } = "";
        public string carModel { get; set; } = "";
        public string carVrm { get; set; } = "";
    }

    public class PersonProfileApiResponse
    {
        public string personRowKey { get; set; } = "";
        public string firstName { get; set; } = "";
        public string surname { get; set; } = "";
        public string nationality { get; set; } = "";
        public string flagUrl { get; set; } = "";
        public List<PersonEvent> events { get; set; } = new();
    }
}