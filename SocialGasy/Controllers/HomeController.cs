using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SocialGasy.Models;

namespace SocialGasy.Controllers;

[Authorize] 
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewBag.FactOfTheDay = "Le recensement de la population est le socle de toute planification nationale. " +
                               "Une donnée fiable aujourd'hui, c'est une meilleure école ou un hôpital demain.";
        
        return View();
    }

    public IActionResult About() => View();

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}