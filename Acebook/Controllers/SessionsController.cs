using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using System.Security.Cryptography;

namespace acebook.Controllers;

public class SessionsController : Controller
{
  private readonly ILogger<SessionsController> _logger;

  public SessionsController(ILogger<SessionsController> logger)
  {
    _logger = logger;
  }

  [Route("/signin")]
  [HttpGet]
  public IActionResult New()
  {
    return View();
  }

  [Route("/signin")]
  [HttpPost]
  public RedirectResult Create(string email, string password)
  {
    AcebookDbContext dbContext = new AcebookDbContext();
    string hashed = HashPassword(password);

    User? user = dbContext.Users.Where(user => user.Email == email).First();
    if (user != null && user.Password == hashed)
    {
      HttpContext.Session.SetInt32("user_id", user.Id);
      return new RedirectResult("/posts");
    }
    else
    {
      return new RedirectResult("/signin");
    }
  }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
    
  private static string HashPassword(string password)
  {
      // Minimal example; prefer ASP.NET Core Identity's PasswordHasher<TUser>
      using var sha256 = SHA256.Create();
      var bytes = System.Text.Encoding.UTF8.GetBytes(password);
      var hash = sha256.ComputeHash(bytes);
      return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
  }
}
