using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using acebook.ActionFilters;
using Microsoft.EntityFrameworkCore;
using Npgsql.PostgresTypes;

namespace acebook.Controllers;

[ServiceFilter(typeof(AuthenticationFilter))]
public class LikesController : Controller
{
    private readonly ILogger<PostsController> _logger;

    public LikesController(ILogger<PostsController> logger)
    {
        _logger = logger;
    }

    [Route("/posts/{id}")] //Toggle like 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleLike(int id)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        var post = dbContext.Posts.Include(p => p.Likes).FirstOrDefault(p => p.Id == id);
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
            dbContext.Likes.Remove(existingLike);
        }
        else
        {
            dbContext.Likes.Add(newLike);

        }
        dbContext.SaveChanges();
        var likeCount = dbContext.Likes.Count(l => l.PostId == id); //Fetching the database like count again so it can be live in the razor 
        return Json(new { likeCount });
    }

    [Route("/comments/{id}/like")] // Toggle like on a comment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleCommentLike(int id)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        var comment = dbContext.Comments.Include(c => c.Likes).FirstOrDefault(c => c.Id == id);
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
            dbContext.Likes.Remove(existingLike);
        }else
        {
            dbContext.Likes.Add(newLike);
        }
        dbContext.SaveChanges();
        var likeCount = dbContext.Likes.Count(l => l.CommentId == id);
        return Json(new { likeCount });
    }

}