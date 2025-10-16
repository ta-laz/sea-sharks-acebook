using acebook.Models;
using acebook.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace acebook.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationsController(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        [Route("/notifications")]
        [HttpGet]
        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("user_id");
            if (userId == null)
                return RedirectToAction("New", "Sessions");

            using var dbContext = new AcebookDbContext();

            var notifications = dbContext.Notifications
                .Where(n => n.ReceiverId == userId)
                .Include(n => n.Sender)
                .OrderByDescending(n => n.CreatedOn)
                .Take(10)
                .ToList();

            ViewBag.Notifications = notifications;
            return View("Index");
        }

        // Get unread notifications for the current user
        [HttpGet("/notifications/unread")]
        public async Task<IActionResult> GetUnread()
        {
            int? userId = HttpContext.Session.GetInt32("user_id");
            if (userId == null)
                return Unauthorized();

            using var dbContext = new AcebookDbContext();

            var notifications = await dbContext.Notifications
                .Where(n => n.ReceiverId == userId && !n.IsRead)
                .Include(n => n.Sender)
                .OrderByDescending(n => n.CreatedOn)
                .ToListAsync();

            return Json(notifications);
        }

        // Mark a single notification as read
        [HttpPost("/notifications/read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            int? userId = HttpContext.Session.GetInt32("user_id");
            if (userId == null)
                return Unauthorized();

            using var dbContext = new AcebookDbContext();

            var notification = await dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.ReceiverId == userId);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        // Send a new notification (called from other controllers)
        [HttpPost("/notifications/send")]
        public async Task<IActionResult> SendNotification(int receiverId, string title, string message)
        {
            int? senderId = HttpContext.Session.GetInt32("user_id");
            using var dbContext = new AcebookDbContext();

            // 1. Save it to the database
            var notification = new Notification
            {
                ReceiverId = receiverId,
                SenderId = senderId,
                Title = title,
                Message = message
            };
            dbContext.Notifications.Add(notification);
            await dbContext.SaveChangesAsync();

            // 2. Push it live via SignalR
            await _hub.Clients.Group($"user-{receiverId}")
                .SendAsync("ReceiveNotification", title, message);

            return Ok("Notification sent.");
        }

        // Mark all as read
        [HttpPost("/notifications/read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int? userId = HttpContext.Session.GetInt32("user_id");
            if (userId == null)
                return Unauthorized();

            using var dbContext = new AcebookDbContext();

            var userNotifications = dbContext.Notifications.Where(n => n.ReceiverId == userId);
            await userNotifications.ForEachAsync(n => n.IsRead = true);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index");

        }
    }
}
