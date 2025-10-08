using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using System.Security.Cryptography;
using Acebook.ViewModels;

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
    public IActionResult Create(SignUpViewModel suvm)
    {

        // if (user.Password != confirmPassword)
        // {
        //     ViewBag.Error = "Passwords do not match.";
        //     return View("New", user);
        // }
        AcebookDbContext dbContext = new AcebookDbContext();
        string hashed = HashPassword(suvm.Password);
        User user = new User
        {
            FirstName = suvm.FirstName,
            LastName = suvm.LastName,
            Email = suvm.Email,
            Password = hashed
        };
        ProfileBio bio = new ProfileBio
        {
            DOB = suvm.DOB
        };
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
        dbContext.ProfileBios.Add(bio);
        dbContext.SaveChanges();

        HttpContext.Session.SetInt32("user_id", user.Id);
        return new RedirectResult("/posts");
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
