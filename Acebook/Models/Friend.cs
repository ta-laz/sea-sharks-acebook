namespace acebook.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Friend
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Requester))]
    public int RequesterId { get; set; }

    [ForeignKey(nameof(Accepter))]
    public int AccepterId { get; set; }
    public FriendStatus Status { get; set; }

    public User Requester { get; set; } = null!;
    public User Accepter { get; set; } = null!;
}

public enum FriendStatus {Pending , Accepted, Declined}