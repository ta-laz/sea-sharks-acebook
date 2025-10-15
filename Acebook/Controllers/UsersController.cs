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
        int? id = HttpContext.Session.GetInt32("user_id");
        if (id != null)
        {
            return Redirect("/posts");
        }
        return View();
    }

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}")]
    [HttpGet]
    public IActionResult Index(int id)
    {
        AcebookDbContext dbContext = new AcebookDbContext();

        // find the user that has the id of the page we're looking at
        // include that user's posts and profile bio details
        var user = dbContext.Users
                  .Include(u => u.ProfileBio)
                  .Include(u => u.Posts)
                  .FirstOrDefault(u => u.Id == id);

        if (user == null)
            return NotFound();

        // retrieve all the posts where the wallid matches the id of the page we're on
        var posts = dbContext.Posts.Where(p => p.WallId == id)
                                   .Include(p => p.User)
                                   .OrderByDescending(p => p.CreatedOn);
        ViewBag.Posts = posts.ToList();

        // search through the friends table, filter for records where the requester and accepter have 
        // the id of the page we're on and the status is accepted
        // take up to 3 friends and make it a list to display it on the user's profile page
        var friends = dbContext.Friends
        .Include(f => f.Requester)
        .Include(f => f.Accepter)
        .Where(f => (f.RequesterId == id || f.AccepterId == id) && f.Status == FriendStatus.Accepted)
        .Take(3)
        .ToList();

        int? currentUserId = HttpContext.Session.GetInt32("user_id");
        ViewBag.CurrentUserId = currentUserId;
        ViewBag.Friends = friends;
        ViewBag.ProfileUserId = id;

        // ✅ Check if logged-in user and viewed user are friends
        bool friendship = dbContext.Friends.Any(f =>
            ((f.RequesterId == currentUserId && f.AccepterId == id) ||
             (f.RequesterId == id && f.AccepterId == currentUserId))
             && f.Status == FriendStatus.Accepted
        );
        ViewBag.Friendship = friendship;

        // ✅ Build list of accepted friendships for the logged-in user
        var acceptedFriendships = dbContext.Friends
            .Where(f => (f.RequesterId == currentUserId || f.AccepterId == currentUserId)
                     && f.Status == FriendStatus.Accepted)
            .ToList();

        var alreadyFriends = new List<int>();
        foreach (var f in acceptedFriendships)
        {
            alreadyFriends.Add(f.RequesterId == currentUserId ? f.AccepterId : f.RequesterId);
        }

        // ✅ Build list of pending requests involving the logged-in user
        var pendingRequests = dbContext.Friends
            .Where(f =>
                (f.RequesterId == currentUserId || f.AccepterId == currentUserId) &&
                f.Status == FriendStatus.Pending)
            .ToList();

        // ✅ Pass both lists to the view
        ViewBag.AlreadyFriends = alreadyFriends;
        ViewBag.PendingRequests = pendingRequests;

        // if the logged in user's id matches the id of the page we're on render the my profile HTML
        if (currentUserId == id)
        {
            return View("MyProfile", user);
        }
        // else render the other profile HTML
        else
        {
            return View("OtherProfile", user);
        }
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

        return View(user);
    }

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}/update")]
    [HttpPost]
    public IActionResult Update(int id, string tagline, string relationshipStatus, string pets, string job)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        var profileBio = dbContext.ProfileBios.Find(id);

        profileBio.Tagline = tagline;
        profileBio.RelationshipStatus = relationshipStatus;
        profileBio.Pets = pets;
        profileBio.Job = job;
        dbContext.SaveChanges();

        if (profileBio == null)
            return NotFound();

        return new RedirectResult($"/users/{id}");
    }
}
