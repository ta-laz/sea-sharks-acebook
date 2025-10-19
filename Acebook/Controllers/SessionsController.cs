using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using Acebook.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace acebook.Controllers;

public class SessionsController : Controller
{
  private readonly ILogger<SessionsController> _logger;
  private readonly IPasswordHasher<User> _hasher;
  private readonly AcebookDbContext _db;

  public SessionsController(ILogger<SessionsController> logger, IPasswordHasher<User> hasher, AcebookDbContext db)
  {
    _logger = logger;
    _hasher = hasher;
    _db = db;
  }

  [Route("/signin")]
  [HttpGet]
  public IActionResult New()
  {
    int? id = HttpContext.Session.GetInt32("user_id");
    if (id != null)
    {
      return Redirect("/posts");
    }
    return View();
  }

  [Route("/signin")]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(SignInViewModel sivm)
  {
    if (!ModelState.IsValid)
    {
      return View("New", sivm);
    }

    User? user = await _db.Users.FirstOrDefaultAsync(user => user.Email == sivm.Email);
    if (user == null)
    {
      ModelState.AddModelError("", "Incorrect email or password.");
      return View("New", sivm);
    }

    var verify = _hasher.VerifyHashedPassword(user, user.Password, sivm.Password);
    if (verify == PasswordVerificationResult.Failed)
    {
        ModelState.AddModelError("", "Incorrect email or password.");
        return View("New", sivm);
    }
    if (verify == PasswordVerificationResult.SuccessRehashNeeded)
    {
        user.Password = _hasher.HashPassword(user, sivm.Password);
        await _db.SaveChangesAsync();
    }

    HttpContext.Session.SetInt32("user_id", user.Id);
    HttpContext.Session.SetString("user_profile_picture", user.ProfilePicturePath ?? "");
    return Redirect("/posts");
  }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }


  [Route("/Signout")]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Signout()
  {
    HttpContext.Session.Clear();

    return RedirectToAction("Index", "Home");
  }
}

  // Legacy hasher
  // private static string HashPassword(string password)
  // {
  //   using var sha256 = SHA256.Create();
  //   var bytes = System.Text.Encoding.UTF8.GetBytes(password);
  //   var hash = sha256.ComputeHash(bytes);
  //   return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
  // }