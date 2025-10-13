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
                               .Include(p => p.User)
                               .Include(p => p.Comments)
                               .Include(p => p.Likes);
    ViewBag.Posts = posts.ToList();
    ViewBag.Posts.Reverse();
    return View();
  }

  // CREATE a Post
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

  // READ a Post (individual post)
  [Route("/posts/{id}")]
  [HttpGet]
  public IActionResult Post(int id)
  {
    AcebookDbContext dbContext = new AcebookDbContext();
    var post = dbContext.Posts.Include(p => p.Comments).Include(p => p.Likes).FirstOrDefault(p => p.Id == id);
    var comments = dbContext.Comments.Include(c => c.User).Where(c => c.PostId == id).ToList();

    ViewBag.post = post;
    ViewBag.comments = comments.ToList();
    ViewBag.comments.Reverse();


    return View(post);
  }
  
  // DELETE a Post
  [Route("/posts/{id}/delete")]
  [HttpPost]
  public IActionResult Delete(int id)
  {
    AcebookDbContext dbContext = new AcebookDbContext();
    Post post = dbContext.Posts.Include(p => p.Comments).Include(p => p.Likes).FirstOrDefault(p => p.Id == id);
    dbContext.Posts.Remove(post);
    dbContext.SaveChanges();

    // Redirect to where the form came from
    
    return RedirectToAction("Index", "Posts");
  }
  


  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}
