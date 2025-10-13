namespace acebook.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


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
    public ICollection<Like>? Likes { get; set; }


    [NotMapped] // prevents EF from treating it as a DB column
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
