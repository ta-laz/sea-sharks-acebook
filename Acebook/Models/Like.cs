namespace acebook.Models;

using System.ComponentModel.DataAnnotations;

public class Like
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PostId { get; set; }
    // Navigation Properties:
    public Post? Post { get; set; }
    public User? User { get; set; }

}
