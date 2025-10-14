using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using System.Security.Cryptography;
using Acebook.ViewModels;

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
  public IActionResult Create(SignInViewModel sivm)
  {
    if (!ModelState.IsValid)
    {
      return View("New", sivm);
    }
    AcebookDbContext dbContext = new AcebookDbContext();
    string hashed = HashPassword(sivm.Password);

    User? user = dbContext.Users.FirstOrDefault(user => user.Email == sivm.Email);
    if (user != null && user.Password == hashed)
    {
      HttpContext.Session.SetInt32("user_id", user.Id);
      if (!string.IsNullOrEmpty(user.ProfilePicturePath))
      {
        HttpContext.Session.SetString("user_profile_picture", user.ProfilePicturePath);
      }
      else
      {
        HttpContext.Session.SetString("user_profile_picture", "");
      }
      return new RedirectResult("/posts");
    }
    else
    {
      ModelState.AddModelError("", "Incorrect email or password.");
      return View("New", sivm);
    }
  }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }

  private static string HashPassword(string password)
  {
    using var sha256 = SHA256.Create();
    var bytes = System.Text.Encoding.UTF8.GetBytes(password);
    var hash = sha256.ComputeHash(bytes);
    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
  }

  [Route("/Signout")]
  [HttpPost]
  public IActionResult Signout()
  {
    HttpContext.Session.Clear();

    return RedirectToAction("Index", "Home");
  }
}
