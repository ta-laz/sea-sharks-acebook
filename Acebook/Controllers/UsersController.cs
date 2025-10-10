using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using System.Security.Cryptography;
using Acebook.ViewModels;
using acebook.ActionFilters;
using Microsoft.EntityFrameworkCore;

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

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}")]
    [HttpGet]
    public IActionResult Index(int id)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        var user = dbContext.Users
                  .Include(u => u.ProfileBio)
                  .Include(u => u.Posts)
                  .FirstOrDefault(u => u.Id == id);

        if (user == null)
            return NotFound();

        var posts = dbContext.Posts.Where(u => u.UserId == id)
                                   .Include(p => p.User);
        ViewBag.Posts = posts.ToList();
        ViewBag.Posts.Reverse();

        return View(user);
    }

    [Route("/users")]
    [HttpPost]
    public IActionResult Create(SignUpViewModel suvm)
    {


        if (!ModelState.IsValid)
        {
            return View("New", suvm);
        }

        AcebookDbContext dbContext = new AcebookDbContext();
        if (dbContext.Users.Any(user => user.Email == suvm.Email))
        {
            ModelState.AddModelError("", "Email already registered.");
            return View("New", suvm);
        }

        string hashed = HashPassword(suvm.Password);
        User user = new User
        {
            FirstName = suvm.FirstName,
            LastName = suvm.LastName,
            Email = suvm.Email,
            Password = hashed
        };
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        ProfileBio bio = new ProfileBio
        {
            UserId = user.Id,
            DOB = suvm.DOB
        };
        dbContext.ProfileBios.Add(bio);
        dbContext.SaveChanges();
        Console.WriteLine($"Session set: {HttpContext.Session.GetInt32("user_id")}");

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

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}/update")]
    [HttpGet]
    public IActionResult Update(int id)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        var user = dbContext.Users
                  .Include(u => u.ProfileBio)
                  .Include(u => u.Posts)
                  .FirstOrDefault(u => u.Id == id);

        if (user == null)
            return NotFound();

        var posts = dbContext.Posts.Where(u => u.UserId == id);

        return View(user);
    }
}
