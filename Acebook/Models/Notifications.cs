using System.ComponentModel.DataAnnotations;

namespace acebook.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public int ReceiverId { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsRead { get; set; } = false;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string? Url { get; set; }
    }
}
