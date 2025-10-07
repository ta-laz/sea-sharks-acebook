using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using System.Security.Cryptography;


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
    public IActionResult Create(User user, string confirmPassword)
    {

        if (user.Password != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match.";
            return View("New", user);
        }
        AcebookDbContext dbContext = new AcebookDbContext();
        string hashed = HashPassword(user.Password);
        user.Password = hashed;
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
        return new RedirectResult("/signin");
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
}
