using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using acebook.ActionFilters;
using Microsoft.EntityFrameworkCore;


namespace acebook.Controllers;

[ServiceFilter(typeof(AuthenticationFilter))]
public class CommentsController : Controller
{
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(ILogger<CommentsController> logger)
    {
        _logger = logger;
    }

    // [Route("/posts")]
    // [HttpGet]
    // public IActionResult Index()
    // {
    //     AcebookDbContext dbContext = new AcebookDbContext();
    //     var posts = dbContext.Posts
    //                                .Include(p => p.User);
    //     ViewBag.Posts = posts.ToList();
    //     ViewBag.Posts.Reverse();

    //     return View();
    // }

    [Route("post/create")]
    [HttpPost]
    public IActionResult Create(int postId, Comment comment, string returnURL)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        AcebookDbContext dbContext = new AcebookDbContext();
        comment.UserId = currentUserId;
        comment.CreatedOn = DateTime.UtcNow;
        comment.PostId = postId;
        dbContext.Comments.Add(comment);
        dbContext.SaveChanges();
    return new RedirectResult($"/posts/{postId}");
    }

}
