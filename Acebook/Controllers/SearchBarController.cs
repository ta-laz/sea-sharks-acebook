using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using System.Security.Cryptography;
using Acebook.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Linq;

namespace acebook.Controllers;

public class SearchBarController : Controller
{
    private readonly ILogger<SearchBarController> _logger;

    public SearchBarController(ILogger<SearchBarController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/Search")]
    public async Task<IActionResult> Index(string SearchString)
    {
        using var dbContext = new AcebookDbContext();

        List<User> userResults = new();
        List<Post> postResults = new();
        List<Comment> commentResults = new();

        if (!string.IsNullOrWhiteSpace(SearchString))
        {
            // Search users
            userResults = await dbContext.Users
                .Where(u => u.FirstName.Contains(SearchString) || u.LastName.Contains(SearchString))
                .ToListAsync();

            // Search posts (and include author)
            postResults = await dbContext.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.Content.Contains(SearchString))
                        //|| p.User.FirstName.Contains(SearchString)
                        //|| p.User.LastName.Contains(SearchString))
                .ToListAsync();

            // Search comments (and include author + post)
            commentResults = await dbContext.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .Include(c => c.Post.Likes)
                .Where(c => c.Content.Contains(SearchString)).ToListAsync();
        }

        ViewBag.UsersResults = userResults;
        ViewBag.PostsResults = postResults;
        ViewBag.CommentsResults = commentResults;
        ViewData["SearchString"] = SearchString;

        return View();
    }
}