namespace acebook.Models;

using System.ComponentModel.DataAnnotations;

public class Friend
{
    [Key]
    public int Id { get; set; }
    public int RequesterId { get; set; }
    public int AccepterId { get; set; }
    public FriendStatus Status {get; set; }
}

public enum FriendStatus {Pending, Accepted, Declined}