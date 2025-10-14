
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
    int currentUserId = HttpContext.Session.GetInt32("user_id").Value;

    var posts = dbContext.Posts
                               .Include(p => p.User)
                               .Include(p => p.Comments)
                                  .ThenInclude(c => c.Likes)
                               .Include(p => p.Likes)
                               .ToList();
    foreach (var post in posts)
        {
      post.UserHasLiked = post.Likes.Any(l => l.UserId == currentUserId);
      if (post.Comments != null)
            {
                foreach (var comment in post.Comments)
                {
          comment.UserHasLiked = comment.Likes.Any(l => l.UserId == currentUserId);
                }
            }
        }

    ViewBag.Posts = posts;
    ViewBag.Posts.Reverse();
    return View();
  }

  // CREATE a Post
  [Route("/posts/create")]
  [HttpPost]
  public IActionResult Create(Post post, string returnUrl, int? WallId = null)
  {
    using var dbContext = new AcebookDbContext();
    int currentUserId = HttpContext.Session.GetInt32("user_id").Value;

    post.UserId = currentUserId;
    post.CreatedOn = DateTime.UtcNow;

    // If WallId is null, default it to the current user’s wall
    post.WallId = WallId ?? currentUserId;

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
    int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
    var post = dbContext.Posts.Include(p => p.Comments).ThenInclude(c => c.Likes).Include(p => p.Likes).FirstOrDefault(p => p.Id == id);
    var comments = dbContext.Comments.Include(c => c.User).Where(c => c.PostId == id).ToList();
    post.UserHasLiked = post.Likes.Any(l => l.UserId == currentUserId);
    foreach (var comment in post.Comments)
    {
      comment.UserHasLiked = comment.Likes.Any(l => l.UserId == currentUserId);
    }
    ViewBag.post = post;
    ViewBag.comments = comments.ToList();
    ViewBag.comments.Reverse();


    return View(post);
  }

  // UPDATE (Edit) a Post
  [Route("/posts/{id}/update")]
    [HttpGet]
    public IActionResult Update(int id)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
    int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
    var post = dbContext.Posts.Include(p => p.Comments).ThenInclude(c => c.Likes).Include(p => p.Likes).FirstOrDefault(p => p.Id == id);

    return View(post);
  }
  
  // UPDATE (Edit) a Post
  [Route("/posts/{id}/update")]
  [HttpPost]
  public IActionResult Update(int id, string content)
  {
    AcebookDbContext dbContext = new AcebookDbContext();
    int? sessionUserId = HttpContext.Session.GetInt32("user_id");
    var post = dbContext.Posts.Find(id);
    // Post post = dbContext.Posts.Include(p => p.Comments).Include(p => p.Likes).FirstOrDefault(p => p.Id == id);
    // if (post.UserId != sessionUserId) // Server-side security (only authors can delete comments)
    // {
    //   return Forbid();
    // }
    // Update the post in the db with the new content
    post.Content = content;
    post.CreatedOn = DateTime.UtcNow;
    dbContext.SaveChanges();

    // Redirect to aquarium
    return RedirectToAction("Post", "Posts", new { id = post.Id });
  }

  // DELETE a Post
  [Route("/posts/{id}/delete")]
  [HttpPost]
  public IActionResult Delete(int id)
  {
    AcebookDbContext dbContext = new AcebookDbContext();
    int? sessionUserId = HttpContext.Session.GetInt32("user_id");
    if (sessionUserId == null)
        return Unauthorized(); // Checks user is logged in
    Post post = dbContext.Posts.Include(p => p.Comments).Include(p => p.Likes).FirstOrDefault(p => p.Id == id);
    if (post.UserId != sessionUserId) // Server-side security (only authors can delete comments)
    {
      return Forbid();
    }
    // Deletes the post from the db 
    dbContext.Posts.Remove(post);
    dbContext.SaveChanges();

    // Redirect to aquarium
    return RedirectToAction("Index", "Posts");
  }
  


  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}
