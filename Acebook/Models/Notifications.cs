using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace acebook.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        public int? SenderId { get; set; }

        [ForeignKey("SenderId")]
        public User? Sender { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        public string? Url { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
