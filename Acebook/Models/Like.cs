namespace acebook.Models;

using System.ComponentModel.DataAnnotations;

public class Like
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? PostId { get; set; } //Nullable 
    
    public int? CommentId { get; set; } //Nullable
    // Navigation Properties:
    public Post? Post { get; set; }
    public User? User { get; set; }

    public Comment? Comment { get; set;}

}
