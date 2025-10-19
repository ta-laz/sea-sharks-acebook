
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
  private readonly AcebookDbContext _db;

  public PostsController(ILogger<PostsController> logger, IHubContext<NotificationHub> hub, AcebookDbContext db)
  {
    _logger = logger;
    _hub = hub;
    _db = db;
  }

  [Route("/posts")]
  [HttpGet]
  public async Task<IActionResult> Index(string? filter)
  {
    int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
    var user = await _db.Users
                  .FirstOrDefaultAsync(u => u.Id == currentUserId);

    var friendIds = await _db.Friends //Logic to pull all of the Id's of the CURRENT users friends
                                      .Where(f => (f.RequesterId == currentUserId || f.AccepterId == currentUserId) && f.Status == FriendStatus.Accepted)
                                      .Select(f => f.RequesterId == currentUserId ? f.AccepterId : f.RequesterId)
                                      .ToListAsync();

    List<Post> posts; 

    if (filter == "friends") //button will trigger a friends search which will pull just the posts of friends that match the friendIds 
    {
      posts = await _db.Posts
                              .Where(p => friendIds.Contains(p.UserId) && p.UserId == p.WallId)
                              .Include(p => p.User)
                              .Include(p => p.Comments)
                                .ThenInclude(c => c.Likes)
                              .Include(p => p.Likes)
                              .ToListAsync();

    }
    else //otherwise generate all posts 
    {
      posts = await _db.Posts
                              .Where(p => p.UserId == p.WallId)
                              .Include(p => p.User)
                              .Include(p => p.Comments)
                                .ThenInclude(c => c.Likes)
                              .Include(p => p.Likes)
                              .ToListAsync();
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
  public async Task<IActionResult> Create(Post post, string returnUrl, IFormFile? postPicture, int? WallId = null)
  {
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
        await postPicture.CopyToAsync(stream);
      }
      post.PostPicturePath = $"/images/post_pics/{fileName}";
    }
    _db.Posts.Add(post);
    await _db.SaveChangesAsync();

    // logic for the notifications is here
    if (post.WallId != post.UserId)
    {
      var poster = await _db.Users.FindAsync(currentUserId);
      var wallOwner = await _db.Users.FindAsync(post.WallId);

      if (poster != null && wallOwner != null)
      {
        string title = "New Post on Your Wall";
        string message = $"{poster.FirstName} posted on your wall.";
        string url = $"/posts/{post.Id}";

        _db.Notifications.Add(new Notification
        {
          ReceiverId = wallOwner.Id,
          SenderId = currentUserId,
          Title = title,
          Message = message,
          Url = url
        });

        await _db.SaveChangesAsync();

        // Send live SignalR notification
        await _hub.Clients.Group($"user-{wallOwner.Id}")
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
  public async Task<IActionResult> Post(int id)
  {
    int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
    var post = await _db.Posts.Include(p => p.Comments).ThenInclude(c => c.Likes).Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == id);
    var comments = await _db.Comments.Include(c => c.User).Where(c => c.PostId == id).OrderBy(c => c.CreatedOn).ToListAsync();
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
  public async Task<IActionResult> Update(int id, string content)
  {
    int? sessionUserId = HttpContext.Session.GetInt32("user_id");
    var post = await _db.Posts.FindAsync(id);
    if (post.UserId != sessionUserId) // Server-side security (only post authors can update posts)
    {
      return Forbid();
    }
    post.Content = content;
    post.CreatedOn = DateTime.UtcNow;
    await _db.SaveChangesAsync();

    // Reload individual post page
    return RedirectToAction("Post", "Posts", new { id = post.Id });
  }
  

  // DELETE a Post
  [Route("/posts/{id}/delete")]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Delete(int id)
  {
    int? sessionUserId = HttpContext.Session.GetInt32("user_id");
    if (sessionUserId == null)
        return Unauthorized(); // Checks user is logged in
    Post post = await _db.Posts.Include(p => p.Comments).Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == id);
    if (post.UserId != sessionUserId && post.WallId != sessionUserId) // Server-side security (only authors or wall owners can delete comments)
    {
      return Forbid();
    }
    // Deletes the post from the db 
    _db.Posts.Remove(post);
    await _db.SaveChangesAsync();

    // Redirect to My Profile:
    return Redirect($"/users/{sessionUserId}");
  }
  



  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}
