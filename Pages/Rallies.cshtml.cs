using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
namespace RallyingIsLife.Pages { public class RallyItem { public int Year {get;set;} public string EventName {get;set;} public string CompetitiveDistance {get;set;} } public class RalliesModel : PageModel { public System.Collections.Generic.List<RallyItem> Rallies {get;set;}=new(); } }
