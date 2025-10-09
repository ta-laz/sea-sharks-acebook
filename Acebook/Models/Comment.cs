namespace acebook.Models;

using System.ComponentModel.DataAnnotations;

public class Comment
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PostId { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedOn { get; set; }

    // Navigation Properties:
    public Post? Post { get; set; }
    public User? User { get; set; }
}
