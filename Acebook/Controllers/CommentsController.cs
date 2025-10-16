using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using acebook.ActionFilters;
using Microsoft.EntityFrameworkCore;
using acebook.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace acebook.Controllers;

[ServiceFilter(typeof(AuthenticationFilter))]
public class CommentsController : Controller
{
    private readonly ILogger<CommentsController> _logger;
    private readonly IHubContext<NotificationHub> _hub;

    public CommentsController(ILogger<CommentsController> logger, IHubContext<NotificationHub> hub)
    {
        _logger = logger;
        _hub = hub;
    }

    // See individual post
    [Route("post/")]
    [HttpGet]
    public IActionResult Index()
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        var posts = dbContext.Posts
                                   .Include(p => p.User);
        ViewBag.Posts = posts.ToList();
        ViewBag.Posts.Reverse();

        return View();
    }

    // CREATE a comment
    [Route("post/create")]
    [HttpPost]
    public IActionResult Create(int postId, Comment comment)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        AcebookDbContext dbContext = new AcebookDbContext();
        comment.UserId = currentUserId;
        comment.CreatedOn = DateTime.UtcNow;
        comment.PostId = postId;
        dbContext.Comments.Add(comment);
        dbContext.SaveChanges();

        // Fetch commenter, post, and post owner
        var commenter = dbContext.Users.Find(currentUserId);
        var post = dbContext.Posts
            .Include(p => p.User)
            .FirstOrDefault(p => p.Id == postId);

        if (post != null && commenter != null)
        {
            if (post.UserId != currentUserId)
            {
                // Notification for someone commenting on your posty
                string title = "New Comment on Your Post";
                string message = $"{commenter.FirstName} commented on your post.";
                string url = $"/posts/{post.Id}";

                dbContext.Notifications.Add(new Notification
                {
                    ReceiverId = post.UserId,
                    Title = title,
                    Message = message,
                    Url = url
                });
                dbContext.SaveChanges();

                _hub.Clients.Group($"user-{post.UserId}")
                    .SendAsync("ReceiveNotification", title, message, url);
            }
            
            if (post.WallId != post.UserId && post.WallId != currentUserId)
            {
                string title = "New Comment on a Post on Your Wall";
                string message = $"{commenter.FirstName} commented on a post on your wall.";

                dbContext.Notifications.Add(new Notification
                {
                    ReceiverId = post.WallId,
                    Title = title,
                    Message = message,
                    Url = $"/posts/{post.Id}"
                });

                _hub.Clients.Group($"user-{post.WallId}")
                    .SendAsync("ReceiveNotification", title, message, $"/posts/{post.Id}");
            }
            // Notify other people who commented on the post
            var otherCommenters = dbContext.Comments
                .Where(c => c.PostId == post.Id && c.UserId != currentUserId)
                .Select(c => c.UserId)
                .Distinct()
                .ToList();

            foreach (var commenterId in otherCommenters)
            {
                if (commenterId == post.UserId || commenterId == post.WallId) continue; // skip post owner (already notified)

                string title = "New Comment on a Post You Commented On";
                string message = $"{commenter.FirstName} also commented on a post you commented on.";
                string url = $"/posts/{post.Id}";

                dbContext.Notifications.Add(new Notification
                {
                    ReceiverId = commenterId,
                    Title = title,
                    Message = message,
                    Url = url
                });

                _hub.Clients.Group($"user-{commenterId}")
                    .SendAsync("ReceiveNotification", title, message, url);
            }

            dbContext.SaveChanges();
        }

        return new RedirectResult($"/posts/{postId}");
    }


    // UPDATE (Edit) a Comment -> submit the editing form and update the db
    [Route("/posts/{postId}/{commentId}/update")]
    [HttpPost]
    public IActionResult Update(int postId, int commentId, string comment_content)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        int? sessionUserId = HttpContext.Session.GetInt32("user_id");
        var comment = dbContext.Comments
            .Include(c => c.Post)
            .FirstOrDefault(c => c.Id == commentId && c.PostId == postId);

        if (comment.UserId != sessionUserId) // Backend security, only comment authors can edit comments
        {
            return Forbid();
        }
        comment.Content = comment_content;
        comment.CreatedOn = DateTime.UtcNow;
        dbContext.SaveChanges();

        // Reload individual post page
        return new RedirectResult($"/posts/{postId}");
    }
    
    

    // DELETE a Comment
    [Route("/posts/{postId}/{commentId}/delete-comment")]
    [HttpPost]
    public IActionResult Delete(int postId, int commentId)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        int? sessionUserId = HttpContext.Session.GetInt32("user_id");
        if (sessionUserId == null) // Checks user is logged in
        {
            return Unauthorized(); 
        }
        Comment comment = dbContext.Comments.Include(c => c.Post).FirstOrDefault(c => c.Id == commentId);
        
        if (comment.UserId != sessionUserId && comment.Post.UserId != sessionUserId) // Server-side security, if the current user is neither the post or the comment author, they can't delete the comment
        {
            return Forbid();
        }
        if (comment == null)
            return NotFound(); // If the comment doesn't exist
        
        // Deletes the post from the db 
        dbContext.Comments.Remove(comment);
        dbContext.SaveChanges();

        // Reload the individual post page
        return new RedirectResult($"/posts/{postId}");
    }

}
