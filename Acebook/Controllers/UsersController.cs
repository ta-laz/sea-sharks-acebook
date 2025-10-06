using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;

namespace acebook.Controllers;

public class UsersController : Controller
{
    private readonly ILogger<UsersController> _logger;

    public UsersController(ILogger<UsersController> logger)
    {
        _logger = logger;
    }

    [Route("/signup")]
    [HttpGet]
    public IActionResult New()
    {
        return View();
    }

    [Route("/users")]
    [HttpPost]
    public RedirectResult Create(User user) {
      AcebookDbContext dbContext = new AcebookDbContext();
      dbContext.Users.Add(user);
      dbContext.SaveChanges();
      return new RedirectResult("/signin");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
