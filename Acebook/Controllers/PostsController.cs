using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using acebook.ActionFilters;

namespace acebook.Controllers;

[ServiceFilter(typeof(AuthenticationFilter))]
public class PostsController : Controller
{
    private readonly ILogger<PostsController> _logger;

    public PostsController(ILogger<PostsController> logger)
    {
        _logger = logger;
    }

    [Route("/posts")]
    [HttpGet]
    public IActionResult Index() {
      AcebookDbContext dbContext = new AcebookDbContext();
      List<Post> posts = dbContext.Posts.ToList();
      ViewBag.Posts = posts;
      return View();
    }

    [Route("/posts")]
    [HttpPost]
    public RedirectResult Create(Post post) {
      AcebookDbContext dbContext = new AcebookDbContext();
      int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
      post.UserId = currentUserId;
      dbContext.Posts.Add(post);
      dbContext.SaveChanges();
      return new RedirectResult("/posts");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
