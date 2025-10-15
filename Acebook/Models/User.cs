namespace acebook.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Index(nameof(Email), IsUnique = true)]
public class User
{
  [Key]
  public int Id { get; set; }
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
  public string? Email { get; set; }
  public string? Password { get; set; }
  public string? ProfilePicturePath { get; set; }

  // Navigation Properties:
  public ICollection<Post>? Posts { get; set; }
  public ICollection<Friend> FriendRequestsSent { get; set; } = new List<Friend>();
  public ICollection<Friend> FriendRequestsReceived { get; set; } = new List<Friend>();
  public ICollection<Comment>? Comments { get; set; } = new List<Comment>();

  public ProfileBio? ProfileBio { get; set; }
}