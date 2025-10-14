namespace acebook.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Post
{
  [Key]
  public int Id { get; set; }
  public string? Content { get; set; }
  public int UserId { get; set; }
  public int WallId { get; set; }
  public DateTime CreatedOn { get; set; }
  public User? User { get; set; }
  public ICollection<Comment>? Comments { get; set; }
  public ICollection<Like>? Likes { get; set; }
  public string? PostPicturePath { get; set; }

  [NotMapped] // prevents EF from treating it as a DB column
  public bool UserHasLiked { get; set; }
  public string FormattedCreatedOn
  {
    get
    {
      int day = CreatedOn.Day;
      string suffix = (day % 10 == 1 && day != 11) ? "st"
                    : (day % 10 == 2 && day != 12) ? "nd"
                    : (day % 10 == 3 && day != 13) ? "rd"
                    : "th";
      return CreatedOn.ToString($"dddd d'{suffix}' MMMM yyyy");
    }
  }

  public bool CheckLength()
  {
    if (this.Content.Length <= 500)
    {
      return true;
    }
    else return false;
  }

  public string FormatPostContent()
  {
    if (!this.CheckLength())
    {
      return $"{this.Content.Substring(0, 500)}...";
    }
    else return null;
  }
}

