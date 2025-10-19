using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using acebook.ActionFilters;
using Microsoft.EntityFrameworkCore;
using Npgsql.PostgresTypes;
using acebook.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace acebook.Controllers;

[ServiceFilter(typeof(AuthenticationFilter))]
public class LikesController : Controller
{
    private readonly ILogger<PostsController> _logger;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly AcebookDbContext _db;

    public LikesController(ILogger<PostsController> logger, IHubContext<NotificationHub> hub, AcebookDbContext db)
    {
        _logger = logger;
        _hub = hub;
        _db = db;
    }

    [Route("/posts/{id}")] //Toggle like 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLike(int id)
    {
        var post = await _db.Posts.Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return NotFound();
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        var existingLike = post.Likes.FirstOrDefault(l => l.UserId == currentUserId); //Checking if a like already exists so it knows if it can toggle 
        var newLike = new Like
        {
            UserId = currentUserId,
            PostId = id,
            CommentId = null
        };
        if (existingLike != null)
        {
            _db.Likes.Remove(existingLike);
        }
        else
        {
            _db.Likes.Add(newLike);

            if (post.UserId != currentUserId)
            {
                var liker = await _db.Users.FindAsync(currentUserId);
                string title = "New Like on Your Post";
                string message = $"{liker.FirstName} liked your post.";
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

        }
        await _db.SaveChangesAsync();
        var likeCount = await _db.Likes.CountAsync(l => l.PostId == id); //Fetching the database like count again so it can be live in the razor 
        return Json(new { likeCount });
    }

    [Route("/comments/{id}/like")] // Toggle like on a comment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCommentLike(int id)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        var comment = await _db.Comments.Include(c => c.Likes).FirstOrDefaultAsync(c => c.Id == id);
        var newLike = new Like
            {
                UserId = currentUserId,
                CommentId = id,
                PostId = null
            };
        if (comment == null)
        {
            return NotFound("Comment not found.");
        }
        var existingLike = comment.Likes.FirstOrDefault(l => l.UserId == currentUserId);
        if (existingLike != null)
        {
            _db.Likes.Remove(existingLike);
        }else
        {
            _db.Likes.Add(newLike);

            // Notify the comment owner (but not if they liked their own comment)
            if (comment.UserId != currentUserId)
            {
                var liker = await _db.Users.FindAsync(currentUserId);
                string title = "New Like on Your Comment";
                string message = $"{liker.FirstName} liked your comment.";
                string url = $"/posts/{comment.PostId}";

                _db.Notifications.Add(new Notification
                {
                    ReceiverId = comment.UserId,
                    SenderId = currentUserId,
                    Title = title,
                    Message = message,
                    Url = url
                });
                await _db.SaveChangesAsync();

                await _hub.Clients.Group($"user-{comment.UserId}")
                    .SendAsync("ReceiveNotification", title, message, url);
            }

        }
        await _db.SaveChangesAsync();
        var likeCount = await _db.Likes.CountAsync(l => l.CommentId == id);
        return Json(new { likeCount });
    }

}