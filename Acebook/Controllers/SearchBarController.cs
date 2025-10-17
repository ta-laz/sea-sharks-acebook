using acebook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace acebook.Controllers;

public class SearchBarController : Controller
{

    private readonly ILogger<SearchBarController> _logger;

    public SearchBarController(ILogger<SearchBarController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/Search")]
    public async Task<IActionResult> Index(string? SearchString, string scope = "all")
    {
        using var db = new AcebookDbContext();
        int? currentUserId = HttpContext.Session.GetInt32("user_id");

        // Normalize scope/filter to one of: users | posts | comments | all
        // string scope = (scope ?? "all").Trim().ToLowerInvariant();
        if (scope is "people") scope = "users"; // accept "people" alias

        // Prepare containers
        var userResults = new List<User>();
        var postResults = new List<Post>();
        var commentResults = new List<Comment>();

        // Empty or whitespace query? Just return empty results for chosen scope(s)
        if (string.IsNullOrWhiteSpace(SearchString))
        {
            ViewBag.UsersResults = (scope is "all" or "users") ? userResults : new List<User>();
            ViewBag.PostsResults = (scope is "all" or "posts") ? postResults : new List<Post>();
            ViewBag.CommentsResults = (scope is "all" or "comments") ? commentResults : new List<Comment>();
            ViewData["SearchString"] = SearchString;
            ViewData["filter"] = scope;
            return View();
        }

        // Build LIKE pattern once
        var term = SearchString.Trim().ToLower();
        var like = $"%{term}%";

        // Run only the queries needed for requested scope
        if (scope is "all" or "users")
        {
            userResults = await db.Users
                .AsNoTracking()
                .Where(u =>
                    EF.Functions.Like(u.FirstName.ToLower(), like) ||
                    EF.Functions.Like(u.LastName.ToLower(), like))
                .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                .Include(u => u.ProfileBio)
                .ToListAsync();
        }

        if (scope is "all" or "posts")
        {
            postResults = await db.Posts
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p =>
                    EF.Functions.Like(p.Content.ToLower(), like) ||
                    EF.Functions.Like(p.User.FirstName.ToLower(), like) ||
                    EF.Functions.Like(p.User.LastName.ToLower(), like))
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        if (scope is "all" or "comments")
        {
            commentResults = await db.Comments
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.Post)
                    .ThenInclude(p => p.User)
                .Include(c => c.Post)
                    .ThenInclude(p => p.Likes)
                .Where(c =>
                    EF.Functions.Like(c.Content.ToLower(), like) ||
                    EF.Functions.Like(c.User.FirstName.ToLower(), like) ||
                    EF.Functions.Like(c.User.LastName.ToLower(), like))
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync();
        }

        var friends = await db.Friends
    .Where(f => (f.RequesterId == currentUserId || f.AccepterId == currentUserId) && f.Status == FriendStatus.Accepted)
    .ToListAsync();

        var pendingRequests = await db.Friends
            .Where(f => f.Status == FriendStatus.Pending &&
                       (f.RequesterId == currentUserId || f.AccepterId == currentUserId))
            .ToListAsync();

        // Prepare a simple lookup for the view
        ViewBag.AlreadyFriends = friends
            .Select(f => f.RequesterId == currentUserId ? f.AccepterId : f.RequesterId)
            .ToList();

        ViewBag.PendingRequests = pendingRequests;

        // Expose only what the view needs
        ViewBag.UsersResults = (scope is "all" or "users") ? userResults : new List<User>();
        ViewBag.PostsResults = (scope is "all" or "posts") ? postResults : new List<Post>();
        ViewBag.CommentsResults = (scope is "all" or "comments") ? commentResults : new List<Comment>();

        ViewData["SearchString"] = SearchString;
        ViewData["filter"] = scope;

        ViewBag.Friends = friends.ToList();
        ViewBag.currentUserId = currentUserId;

        return View(userResults);
    }
}