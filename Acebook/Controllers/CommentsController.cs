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
    private readonly AcebookDbContext _db;

    public CommentsController(ILogger<CommentsController> logger, IHubContext<NotificationHub> hub, AcebookDbContext db)
    {
        _logger = logger;
        _hub = hub;
        _db = db;
    }

    // See individual post
    [Route("post/")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var posts = _db.Posts
                                   .Include(p => p.User);
        ViewBag.Posts = await posts.ToListAsync();
        ViewBag.Posts.Reverse();

        return View();
    }

    // CREATE a comment
    [Route("post/create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int postId, Comment comment)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        comment.UserId = currentUserId;
        comment.CreatedOn = DateTime.UtcNow;
        comment.PostId = postId;
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        // Fetch commenter, post, and post owner
        var commenter = await _db.Users.FindAsync(currentUserId);
        var post = await _db.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post != null && commenter != null)
        {
            if (post.UserId != currentUserId)
            {
                // Notification for someone commenting on your posty
                string title = "New Comment on Your Post";
                string message = $"{commenter.FirstName} commented on your post.";
                string url = $"/posts/{post.Id}";

                _db.Notifications.Add(new Notification
                {
                    ReceiverId = post.UserId,
                    SenderId = currentUserId,
                    Title = title,
                    Message = message,
                    Url = url
                });
                await _db.SaveChangesAsync();

                await _hub.Clients.Group($"user-{post.UserId}")
                    .SendAsync("ReceiveNotification", title, message, url);
            }
            
            if (post.WallId != post.UserId && post.WallId != currentUserId)
            {
                string title = "New Comment on a Post on Your Wall";
                string message = $"{commenter.FirstName} commented on a post on your wall.";

                _db.Notifications.Add(new Notification
                {
                    ReceiverId = post.WallId,
                    SenderId = currentUserId,
                    Title = title,
                    Message = message,
                    Url = $"/posts/{post.Id}"
                });

                await _hub.Clients.Group($"user-{post.WallId}")
                    .SendAsync("ReceiveNotification", title, message, $"/posts/{post.Id}");
            }
            // Notify other people who commented on the post
            var otherCommenters = await _db.Comments
                .Where(c => c.PostId == post.Id && c.UserId != currentUserId)
                .Select(c => c.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var commenterId in otherCommenters)
            {
                if (commenterId == post.UserId || commenterId == post.WallId) continue; // skip post owner (already notified)

                string title = "New Comment on a Post You Commented On";
                string message = $"{commenter.FirstName} also commented on a post you commented on.";
                string url = $"/posts/{post.Id}";

                _db.Notifications.Add(new Notification
                {
                    ReceiverId = commenterId,
                    SenderId = currentUserId,
                    Title = title,
                    Message = message,
                    Url = url
                });

                await _hub.Clients.Group($"user-{commenterId}")
                    .SendAsync("ReceiveNotification", title, message, url);
            }

            await _db.SaveChangesAsync();
        }

        return new RedirectResult($"/posts/{postId}");
    }


    // UPDATE (Edit) a Comment -> submit the editing form and update the db
    [Route("/posts/{postId}/{commentId}/update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int postId, int commentId, string comment_content)
    {
        int? sessionUserId = HttpContext.Session.GetInt32("user_id");
        var comment = await _db.Comments
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.PostId == postId);

        if (comment.UserId != sessionUserId) // Backend security, only comment authors can edit comments
        {
            return Forbid();
        }
        comment.Content = comment_content;
        comment.CreatedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Reload individual post page
        return new RedirectResult($"/posts/{postId}");
    }
    
    

    // DELETE a Comment
    [Route("/posts/{postId}/{commentId}/delete-comment")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int postId, int commentId)
    {
        int? sessionUserId = HttpContext.Session.GetInt32("user_id");
        if (sessionUserId == null) // Checks user is logged in
        {
            return Unauthorized(); 
        }
        Comment comment = await _db.Comments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == commentId);
        
        if (comment.UserId != sessionUserId && comment.Post.UserId != sessionUserId && comment.Post.WallId != sessionUserId) // Server-side security, if the current user is neither the post or the comment author, they can't delete the comment
        {
            return Forbid();
        }
        if (comment == null)
            return NotFound(); // If the comment doesn't exist
        
        // Deletes the post from the db 
        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();

        // Reload the individual post page
        return new RedirectResult($"/posts/{postId}");
    }

}
