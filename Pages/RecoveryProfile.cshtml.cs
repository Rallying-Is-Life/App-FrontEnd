using Microsoft.AspNetCore.Mvc.RazorPages; using System.Net.Http.Json; using Microsoft.Extensions.Configuration;
namespace App_FrontEnd.Pages;
public class RecoveryProfile : PageModel
{
    public string Name{get;set;}=""; public string? Website{get;set;} public string RowKey{get;set;}=""; public List<RecoveryEvent> Events{get;set;}=new(); public async Task OnGetAsync() { var id=Request.Query["id"].ToString(); RowKey=id; var baseUrl = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()["apiRallyingIsLifeBaseURL"]?.TrimEnd('/') ?? ""; using var c=new HttpClient(); var data=await c.GetFromJsonAsync<RecoveryResp>($"{baseUrl}/profiles/recovery?id={id}"); if(data!=null){Name=data.name??"";Website=data.website;Events=data.events??new();} }
    public class RecoveryEvent{public string? pointType{get;set;} public int? stageNumber{get;set;} public int? year{get;set;} public string? eventName{get;set;} public string? stageName{get;set;}}
    public class RecoveryResp{public string? name{get;set;} public string? website{get;set;} public List<RecoveryEvent>? events{get;set;}}
}
