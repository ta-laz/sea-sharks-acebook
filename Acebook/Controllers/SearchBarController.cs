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

    [Route("/Search")]
    [HttpGet]
    public async Task<IActionResult> Index(string SearchString)
    {
        using var dbContext = new AcebookDbContext();

        var users = dbContext.Users
            .Include(u => u.Posts)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchString))
        {
            users = users.Where(u =>
                u.Posts.Any(p => p.Content.Contains(SearchString)) ||
                u.FirstName.Contains(SearchString) ||
                u.LastName.Contains(SearchString)
            );
        }

        ViewData["SearchString"] = SearchString;
        return View(await users.ToListAsync());
    }
}
