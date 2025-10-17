
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using acebook.ActionFilters;
using Microsoft.EntityFrameworkCore;
using acebook.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace acebook.Controllers;

[ServiceFilter(typeof(AuthenticationFilter))]
public class PostsController : Controller
{
  private readonly ILogger<PostsController> _logger;
  private readonly IHubContext<NotificationHub> _hub;

  public PostsController(ILogger<PostsController> logger, IHubContext<NotificationHub> hub)
  {
    _logger = logger;
    _hub = hub;
  }

  [Route("/posts")]
  [HttpGet]
  public IActionResult Index(string? filter)
  {
    AcebookDbContext dbContext = new AcebookDbContext();
    int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
    var user = dbContext.Users
                  .FirstOrDefault(u => u.Id == currentUserId);

    var friendIds = dbContext.Friends //Logic to pull all of the Id's of the CURRENT users friends
                                      .Where(f => (f.RequesterId == currentUserId || f.AccepterId == currentUserId) && f.Status == FriendStatus.Accepted)
                                      .Select(f => f.RequesterId == currentUserId ? f.AccepterId : f.RequesterId)
                                      .ToList();

    List<Post> posts; 

    if (filter == "friends") //button will trigger a friends search which will pull just the posts of friends that match the friendIds 
    {
      posts = dbContext.Posts
                              .Where(p => friendIds.Contains(p.UserId) && p.UserId == p.WallId)
                              .Include(p => p.User)
                              .Include(p => p.Comments)
                                .ThenInclude(c => c.Likes)
                              .Include(p => p.Likes)
                              .ToList();

    }
    else //otherwise generate all posts 
    {
      posts = dbContext.Posts
                               .Where(p => p.UserId == p.WallId)
                               .Include(p => p.User)
                               .Include(p => p.Comments)
                                  .ThenInclude(c => c.Likes)
                               .Include(p => p.Likes)
                               .ToList();
    }

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
    ViewBag.Filter = filter ?? "all"; //So the index.html knows which one is currently active (all or friends) 
    return View(user);
  }

  // CREATE a Post
  [Route("/posts/create")]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Create(Post post, string returnUrl, IFormFile? postPicture, int? WallId = null)
  {
    using var dbContext = new AcebookDbContext();
    int currentUserId = HttpContext.Session.GetInt32("user_id").Value;

    post.UserId = currentUserId;
    post.CreatedOn = DateTime.UtcNow;

    // If WallId is null, default it to the current user’s wall
    post.WallId = WallId ?? currentUserId;

    if (postPicture != null)
    {
      var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/post_pics");
      var fileName = Guid.NewGuid().ToString() + Path.GetExtension(postPicture.FileName);
      var filePath = Path.Combine(uploadsFolder, fileName);

      using (var stream = new FileStream(filePath, FileMode.Create))
      {
        postPicture.CopyTo(stream);
      }
      post.PostPicturePath = $"/images/post_pics/{fileName}";
    }
    dbContext.Posts.Add(post);
    dbContext.SaveChanges();

    // logic for the notifications is here
    if (post.WallId != post.UserId)
    {
      var poster = dbContext.Users.Find(currentUserId);
      var wallOwner = dbContext.Users.Find(post.WallId);

      if (poster != null && wallOwner != null)
      {
        string title = "New Post on Your Wall";
        string message = $"{poster.FirstName} posted on your wall.";
        string url = $"/posts/{post.Id}";

        dbContext.Notifications.Add(new Notification
        {
          ReceiverId = wallOwner.Id,
          SenderId = currentUserId,
          Title = title,
          Message = message,
          Url = url
        });

        dbContext.SaveChanges();

        // Send live SignalR notification
        _hub.Clients.Group($"user-{wallOwner.Id}")
            .SendAsync("ReceiveNotification", title, message, url);
      }
    }


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
    var comments = dbContext.Comments.Include(c => c.User).Where(c => c.PostId == id).OrderBy(c => c.CreatedOn).ToList();
    post.UserHasLiked = post.Likes.Any(l => l.UserId == currentUserId);
    foreach (var comment in post.Comments)
    {
      comment.UserHasLiked = comment.Likes.Any(l => l.UserId == currentUserId);
    }
    ViewBag.post = post;
    ViewBag.comments = comments;


    return View(post);
  }


  // UPDATE (Edit) a Post -> submit the editing form and update the db
  [Route("/posts/{id}/update")]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Update(int id, string content)
  {
    AcebookDbContext dbContext = new AcebookDbContext();
    int? sessionUserId = HttpContext.Session.GetInt32("user_id");
    var post = dbContext.Posts.Find(id);
    if (post.UserId != sessionUserId) // Server-side security (only post authors can update posts)
    {
      return Forbid();
    }
    post.Content = content;
    post.CreatedOn = DateTime.UtcNow;
    dbContext.SaveChanges();

    // Reload individual post page
    return RedirectToAction("Post", "Posts", new { id = post.Id });
  }
  

  // DELETE a Post
  [Route("/posts/{id}/delete")]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Delete(int id)
  {
    AcebookDbContext dbContext = new AcebookDbContext();
    int? sessionUserId = HttpContext.Session.GetInt32("user_id");
    if (sessionUserId == null)
        return Unauthorized(); // Checks user is logged in
    Post post = dbContext.Posts.Include(p => p.Comments).Include(p => p.Likes).FirstOrDefault(p => p.Id == id);
    if (post.UserId != sessionUserId || post.WallId != sessionUserId) // Server-side security (only authors or wall owners can delete comments)
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
