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
        private readonly AcebookDbContext _db;

        public NotificationsController(IHubContext<NotificationHub> hub, AcebookDbContext db)
        {
            _hub = hub;
            _db = db;
        }

        [Route("/notifications")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("user_id");
            if (userId == null)
                return RedirectToAction("New", "Sessions");


            var notifications = await _db.Notifications
                .Where(n => n.ReceiverId == userId)
                .Include(n => n.Sender)
                .OrderByDescending(n => n.CreatedOn)
                .Take(10)
                .ToListAsync();

            ViewBag.Notifications = notifications;
            return View("Index");
        }

        [HttpGet("/notifications/unread")]
        public async Task<IActionResult> GetUnread()
        {
            int? userId = HttpContext.Session.GetInt32("user_id");
            if (userId == null)
                return Unauthorized();


            var notifications = await _db.Notifications
                .Where(n => n.ReceiverId == userId && !n.IsRead)
                .Include(n => n.Sender)
                .OrderByDescending(n => n.CreatedOn)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Url,
                    n.CreatedOn,
                    Sender = n.Sender == null ? null : new
                    {
                        n.Sender.FirstName,
                        n.Sender.LastName,
                        n.Sender.ProfilePicturePath
                    }
                })
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


            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.ReceiverId == userId);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            await _db.SaveChangesAsync();

            return Ok();
        }

        // Send a new notification (called from other controllers)
        [HttpPost("/notifications/send")]
        public async Task<IActionResult> SendNotification(int receiverId, string title, string message, string? url)
        {
            int? senderId = HttpContext.Session.GetInt32("user_id");

            // 1. Save it to the database
            var notification = new Notification
            {
                ReceiverId = receiverId,
                SenderId = senderId,
                Url = url,
                Title = title,
                Message = message
            };
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            // 2. Push it live via SignalR
            await _hub.Clients.Group($"user-{receiverId}")
                .SendAsync("ReceiveNotification", title, message, url);

            return Ok("Notification sent.");
        }

        // Mark all as read
        [HttpPost("/notifications/read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int? userId = HttpContext.Session.GetInt32("user_id");
            if (userId == null)
                return Unauthorized();


            var userNotifications = _db.Notifications.Where(n => n.ReceiverId == userId);
            await userNotifications.ForEachAsync(n => n.IsRead = true);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");

        }
    }
}
