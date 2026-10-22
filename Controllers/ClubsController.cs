using Microsoft.AspNetCore.Mvc;
namespace App_FrontEnd.Controllers;
public class ClubsController : Controller
{
    [HttpGet("/Clubs")]
    public IActionResult Index() => View();
}
