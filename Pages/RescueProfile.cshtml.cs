using Microsoft.AspNetCore.Mvc.RazorPages; using System.Net.Http.Json;
namespace App_FrontEnd.Pages;
public class RescueProfile : PageModel
{
    public string Name{get;set;}=""; public string? Website{get;set;} public string RowKey{get;set;}=""; public List<RescueEvent> Events{get;set;}=new();
    public async Task OnGetAsync() { var id=Request.Query["id"].ToString(); RowKey=id; var baseUrl=(Environment.GetEnvironmentVariable("apiRallyingIsLifeBaseURL")??"https://dev-rallyingislife-api.azurewebsites.net/").TrimEnd('/'); using var c=new HttpClient(); var data=await c.GetFromJsonAsync<RescueResp>($"{baseUrl}/profiles/rescue?id={id}"); if(data!=null){Name=data.name??"";Website=data.website;Events=data.events??new();} }
    public class RescueEvent{public string? pointType{get;set;} public int? stageNumber{get;set;} public int? year{get;set;} public string? eventName{get;set;} public string? stageName{get;set;}}
    public class RescueResp{public string? name{get;set;} public string? website{get;set;} public List<RescueEvent>? events{get;set;}}
}
