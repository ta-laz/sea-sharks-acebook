using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using acebook.ActionFilters;
using Microsoft.EntityFrameworkCore;

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
  public IActionResult Index()
  {
    AcebookDbContext dbContext = new AcebookDbContext();
    var posts = dbContext.Posts
                               .Include(p => p.User);
    ViewBag.Posts = posts.ToList();
    ViewBag.Posts.Reverse();
    Console.WriteLine($"Session check: {HttpContext.Session.GetInt32("user_id")}");

    return View();
  }

  [Route("/posts/create")]
  [HttpPost]
  public IActionResult Create(Post post, string returnUrl)
  {
    using var dbContext = new AcebookDbContext();
    int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
    post.UserId = currentUserId;
    post.CreatedOn = DateTime.UtcNow;
    dbContext.Posts.Add(post);
    dbContext.SaveChanges();

    // Redirect to where the form came from
    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
      return Redirect(returnUrl);
    return RedirectToAction("Index", "Posts");
  }

  [Route("/posts/{id}")]
  [HttpGet]
  public IActionResult Post(int id)
  {
    AcebookDbContext dbContext = new AcebookDbContext();
    var post = dbContext.Posts.Include(p => p.Comments).FirstOrDefault(p => p.Id == id);
    var comments = dbContext.Comments.Include(c => c.User).Where(c => c.PostId == id);
    // var comments = comments.Reverse();
    ViewBag.post = post;
    ViewBag.comments = comments.ToList();
    ViewBag.comments.Reverse();

    return View(post);
  }


  //   [Route("/post")]
  //   [HttpGet]
  //   public IActionResult Post(int id) {
  //         AcebookDbContext dBContext = new AcebookDbContext();
  //         Post? indiPost = dBContext.Posts.Include(p => p.User).FirstOrDefault(p => p.Id == id);
  //         if (indiPost == null)
  //         {
  //             return new RedirectResult("/posts");
  //         }
  //         return View(indiPost);
  // }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}
