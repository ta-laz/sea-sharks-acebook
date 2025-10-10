namespace acebook.Models;

using System.ComponentModel.DataAnnotations;

public class ProfileBio
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Tagline { get; set; }
    public DateOnly DOB { get; set; }
    public string? RelationshipStatus { get; set; }
    public string? Pets { get; set; }
    public string? Job { get; set; }
    public User? user { get; set; }

    public int Age
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            int age = today.Year - DOB.Year;
            if (DOB > today.AddYears(-age)) age--;
            return age;
        }
    }
}
