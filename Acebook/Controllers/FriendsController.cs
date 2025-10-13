using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using System.Security.Cryptography;
using Acebook.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace acebook.Controllers;

public class FriendsController : Controller
{
    private readonly ILogger<FriendsController> _logger;

    public FriendsController(ILogger<FriendsController> logger)
    {
        _logger = logger;
    }

    [Route("/friends")]
    [HttpGet]
    public IActionResult Index(string? SearchQuery)
    {
        AcebookDbContext dbContext = new AcebookDbContext();

        int? currentUserId = HttpContext.Session.GetInt32("user_id");

        var friends = dbContext.Friends
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

        var receivedRequests = dbContext.Friends
        .Include(f => f.Requester)
        .Include(f => f.Accepter)
        .Where(f => f.AccepterId == currentUserId && f.Status == FriendStatus.Pending);

        var sentRequests = dbContext.Friends
        .Include(f => f.Requester)
        .Include(f => f.Accepter)
        .Where(f => f.RequesterId == currentUserId && f.Status == FriendStatus.Pending);

        ViewBag.Friends = friends.ToList();
        ViewBag.currentUserId = currentUserId;
        ViewBag.ReceivedRequests = receivedRequests.ToList();
        ViewBag.SentRequests = sentRequests.ToList();
        ViewBag.SearchQuery = SearchQuery;

        return View();
    }


    [Route("/friends/{id}")]
    [HttpGet]
    public IActionResult ViewUserFriends(int id, string? SearchQuery)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        int? currentUserId = HttpContext.Session.GetInt32("user_id");

        User user = dbContext.Users.Find(id);

        var friends = dbContext.Friends
            .Include(f => f.Requester)
            .Include(f => f.Accepter)
            .Where(f => (f.RequesterId == id || f.AccepterId == id) && f.Status == FriendStatus.Accepted);

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

        ViewBag.user = user;
        ViewBag.friends = friends.ToList();
        ViewBag.currentUserId = currentUserId;

        // variable to see if we are already friends. 
        // if currentUserId and friend.Id have a confirmed relationship

        var relevantFriendships = dbContext.Friends.Where(f =>
                                (f.RequesterId == currentUserId || f.AccepterId == currentUserId)
                                && f.Status == FriendStatus.Accepted)
                                .ToList();

        
        var AlreadyFriends = new List<int>(); // list of IDs of people you're friends with

        // check which id needs to be added into the list above
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

        var PendingRequests = dbContext.Friends
                    .Where(f => (f.RequesterId == currentUserId || f.AccepterId == currentUserId)
                    && f.Status == FriendStatus.Pending);

        ViewBag.PendingRequests = PendingRequests.ToList();        ViewBag.AlreadyFriends = AlreadyFriends;

        return View();
    }



    [Route("/friends/remove")]
    [HttpPost]
    public IActionResult Remove(int friendId)
    {
        AcebookDbContext dbContext = new AcebookDbContext();

        var friend = dbContext.Friends.Find(friendId);
        dbContext.Friends.Remove(friend);
        dbContext.SaveChanges();

        return RedirectToAction("Index");
    }

    [Route("/friends/accept")]
    [HttpPost]
    public IActionResult Accept(int friendId)
    {
        AcebookDbContext dbContext = new AcebookDbContext();

        var friend = dbContext.Friends.Find(friendId);
        friend.Status = FriendStatus.Accepted;
        dbContext.SaveChanges();

        return RedirectToAction("Index");
    }

    [Route("/friends/send")]
    [HttpPost]
    public IActionResult AddFriend(int receiverId)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        int? currentUserId = HttpContext.Session.GetInt32("user_id");

        var existingFriendship = dbContext.Friends.FirstOrDefault(f =>
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

            dbContext.Friends.Add(newRequest);
            dbContext.SaveChanges();

        }
        return Redirect(Request.Headers["Referer"].ToString());
    }
}
