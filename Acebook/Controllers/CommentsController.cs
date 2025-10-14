using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using acebook.ActionFilters;
using Microsoft.EntityFrameworkCore;


namespace acebook.Controllers;

[ServiceFilter(typeof(AuthenticationFilter))]
public class CommentsController : Controller
{
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(ILogger<CommentsController> logger)
    {
        _logger = logger;
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
