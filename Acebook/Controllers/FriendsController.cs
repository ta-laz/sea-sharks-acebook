using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using System.Security.Cryptography;
using Acebook.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using acebook.ActionFilters;
using acebook.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace acebook.Controllers;
[ServiceFilter(typeof(AuthenticationFilter))]
public class FriendsController : Controller
{
    private readonly ILogger<FriendsController> _logger;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly AcebookDbContext _db;


    public FriendsController(ILogger<FriendsController> logger, IHubContext<NotificationHub> hub, AcebookDbContext db)
    {
        _logger = logger;
        _hub = hub;
        _db = db;
    }

    [Route("/friends")]
    [HttpGet]
    public async Task<IActionResult> Index(string? SearchQuery)
    {

        int? currentUserId = HttpContext.Session.GetInt32("user_id");

        var friends = _db.Friends
        .Include(f => f.Requester)
        .Include(f => f.Accepter)
        .Where(f => (f.RequesterId == currentUserId || f.AccepterId == currentUserId) && f.Status == FriendStatus.Accepted);

        // The logic bit for the search query
        if (!string.IsNullOrEmpty(SearchQuery))
        {
            string loweredSearch = SearchQuery.ToLower();

            friends = friends.Where(f =>
                (f.RequesterId == currentUserId &&
                    (f.Accepter.FirstName.ToLower().Contains(loweredSearch) ||
                     f.Accepter.LastName.ToLower().Contains(loweredSearch)))
                ||
                (f.AccepterId == currentUserId &&
                    (f.Requester.FirstName.ToLower().Contains(loweredSearch) ||
                     f.Requester.LastName.ToLower().Contains(loweredSearch)))
            );
        }

        var receivedRequests = _db.Friends
        .Include(f => f.Requester)
        .Include(f => f.Accepter)
        .Where(f => f.AccepterId == currentUserId && f.Status == FriendStatus.Pending);

        var sentRequests = _db.Friends
        .Include(f => f.Requester)
        .Include(f => f.Accepter)
        .Where(f => f.RequesterId == currentUserId && f.Status == FriendStatus.Pending);

        ViewBag.Friends = await friends.ToListAsync();
        ViewBag.currentUserId = currentUserId;
        ViewBag.ReceivedRequests = await receivedRequests.ToListAsync();
        ViewBag.SentRequests = await sentRequests.ToListAsync();
        ViewBag.SearchQuery = SearchQuery;

        return View();
    }


    [Route("/friends/{id}")]
    [HttpGet]
    public async Task<IActionResult> ViewUserFriends(int id, string? SearchQuery)
    {
        int? currentUserId = HttpContext.Session.GetInt32("user_id");

        // get a user object back - this is the user whose friend list you are viewing
        User user = await _db.Users.FindAsync(id);

        //check who the user is friends with
        var friends = _db.Friends
            .Include(f => f.Requester)
            .Include(f => f.Accepter)
            .Where(f => (f.RequesterId == id || f.AccepterId == id) && f.Status == FriendStatus.Accepted);

        // logic for the search functionality
        if (!string.IsNullOrEmpty(SearchQuery))
        {
            string loweredSearch = SearchQuery.ToLower();

            friends = friends.Where(f =>
                (f.RequesterId == id &&
                    (f.Accepter.FirstName.ToLower().Contains(loweredSearch) ||
                     f.Accepter.LastName.ToLower().Contains(loweredSearch)))
                ||
                (f.AccepterId == id &&
                    (f.Requester.FirstName.ToLower().Contains(loweredSearch) ||
                     f.Requester.LastName.ToLower().Contains(loweredSearch)))
            );
        }

        // this is where the logic for checking if I as the user am friends with people on the friend list
        // first, we pick out who our accepted friends are, and put them in a list
        var relevantFriendships = await _db.Friends.Where(f =>
                                (f.RequesterId == currentUserId || f.AccepterId == currentUserId)
                                && f.Status == FriendStatus.Accepted)
                                .ToListAsync();

        // here we create an empty new list, where we will check if people from the friend list displayed, are also already our friends
        var AlreadyFriends = new List<int>(); // list of IDs of people you're friends with

        // check which id needs to be added into the list above - here the comparison happens. Person in other users friend list, are they already my friend? If yes, then add them to the list above.
        foreach (var friendship in relevantFriendships)
        {
            if (friendship.RequesterId == currentUserId)
            {
                AlreadyFriends.Add(friendship.AccepterId);
            }
            else
            {
                AlreadyFriends.Add(friendship.RequesterId);
            }
        }

        // this pulls out who we have a pending relationship with
        var PendingRequests = _db.Friends
                    .Where(f => (f.RequesterId == currentUserId || f.AccepterId == currentUserId)
                    && f.Status == FriendStatus.Pending);

        // bits in viewbag to make us of inside html
        ViewBag.user = user;
        ViewBag.friends = await friends.ToListAsync();
        ViewBag.currentUserId = currentUserId;
        ViewBag.PendingRequests = await PendingRequests.ToListAsync();
        ViewBag.AlreadyFriends = AlreadyFriends;

        return View();
    }


    [Route("/friends/remove")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int friendId, string returnUrl)
    {
        int? currentUserId = HttpContext.Session.GetInt32("user_id");
        if (currentUserId == null) return RedirectToAction("Index"); // sanity check


        // Find the Friend entity where the current user and friendId match
        var friend = _db.Friends
            .FirstOrDefault(f =>
                (f.RequesterId == currentUserId && f.AccepterId == friendId) ||
                (f.RequesterId == friendId && f.AccepterId == currentUserId)
            );

        if (friend == null)
        {
            // Handle error: friendship not found
            return RedirectToAction("Index");
        }

        _db.Friends.Remove(friend);
        await _db.SaveChangesAsync();

        // Redirect to where the form came from
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index");

        // return RedirectToAction("Index"); // or redirect back to the profile page
    }


    [Route("/friends/accept")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int friendId)
    {
        int? currentUserId = HttpContext.Session.GetInt32("user_id");

        var friend = await _db.Friends
            .Include(f => f.Requester)
            .Include(f => f.Accepter)
            .FirstOrDefaultAsync(f => f.Id == friendId);

        if (friend == null)
        {
            // no matching friendship found
            _logger.LogWarning("Tried to accept friendId {FriendId}, but no record found", friendId);
            return RedirectToAction("Index");
        }

        friend.Status = FriendStatus.Accepted;
        await _db.SaveChangesAsync();

        // double-check navigation props
        var accepter = friend.Accepter ?? await _db.Users.FindAsync(friend.AccepterId);
        var requester = friend.Requester ?? await _db.Users.FindAsync(friend.RequesterId);

        if (requester != null && accepter != null)
        {
            string title = "Friend Request Accepted";
            string message = $"{accepter.FirstName} accepted your friend request.";

            _db.Notifications.Add(new Notification
            {
                ReceiverId = requester.Id,
                SenderId = currentUserId,
                Title = title,
                Message = message,
                Url = $"/users/{accepter.Id}"
            });
            await _db.SaveChangesAsync();

            await _hub.Clients.Group($"user-{requester.Id}")
                .SendAsync("ReceiveNotification", title, message, $"/users/{accepter.Id}");
        }
        else
        {
            _logger.LogWarning("Requester or accepter user was null for friendId {FriendId}", friendId);
        }

        return RedirectToAction("Index");
    }


    [Route("/friends/add")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFriend(int receiverId)
    {
        int? currentUserId = HttpContext.Session.GetInt32("user_id");

        var existingFriendship = await _db.Friends.FirstOrDefaultAsync(f =>
            (f.RequesterId == currentUserId && f.AccepterId == receiverId) ||
            (f.RequesterId == receiverId && f.AccepterId == currentUserId));

        if (existingFriendship == null)
        {
            var newRequest = new Friend
            {
                RequesterId = currentUserId.Value,
                AccepterId = receiverId,
                Status = FriendStatus.Pending
            };

            _db.Friends.Add(newRequest);
            await _db.SaveChangesAsync();

            // sends notification that friend request was requested
            var sender = await _db.Users.FindAsync(currentUserId);
            string title = "New Friend Request";
            string message = $"{sender.FirstName} sent you a friend request.";

            _db.Notifications.Add(new Notification
            {
                ReceiverId = receiverId,
                SenderId = currentUserId,
                Title = title,
                Message = message,
                Url = "/friends"

            });
            await _db.SaveChangesAsync();

            await _hub.Clients.Group($"user-{receiverId}")
                .SendAsync("ReceiveNotification", title, message, "/friends");
        }
        // this is used to pull information from the headers to redirect you to where you just were
        return Redirect(Request.Headers["Referer"].ToString());
    }
}
