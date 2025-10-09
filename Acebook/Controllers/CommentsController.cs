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

    // [Route("/post")]
    [HttpPost]
    public IActionResult Create(Comment comment)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        comment.UserId = currentUserId;
        comment.CreatedOn = DateTime.UtcNow;
        //comment.PostId = PostId;
        dbContext.Comments.Add(comment);
        dbContext.SaveChanges();
        return View();
    }

    // [Route("/post")]
    // [HttpGet]
    // public IActionResult Post(int id)
    // {
    //     AcebookDbContext dBContext = new AcebookDbContext();
    //     Post? indiPost = dBContext.Posts.Include(p => p.User).FirstOrDefault(p => p.Id == id);
    //     if (indiPost == null)
    //     {
    //         return new RedirectResult("/posts");
    //     }
    //     return View(indiPost);
    // }

    // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // public IActionResult Error()
    // {
    //     return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    // }
}
