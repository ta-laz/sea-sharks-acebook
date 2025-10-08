namespace acebook.Models;

using System.ComponentModel.DataAnnotations;

public class Post
{
  [Key]
  public int Id { get; set; }
  public string? Content { get; set; }
  public int UserId { get; set; }
  public int WallId { get; set; }
  public DateTime CreatedOn { get; set; }
  public User? User { get; set; }

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
}

