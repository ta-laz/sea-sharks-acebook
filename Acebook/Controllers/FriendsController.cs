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

        // get a user object back - this is the user whose friend list you are viewing
        User user = dbContext.Users.Find(id);

        //check who the user is friends with
        var friends = dbContext.Friends
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
        var relevantFriendships = dbContext.Friends.Where(f =>
                                (f.RequesterId == currentUserId || f.AccepterId == currentUserId)
                                && f.Status == FriendStatus.Accepted)
                                .ToList();

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
        var PendingRequests = dbContext.Friends
                    .Where(f => (f.RequesterId == currentUserId || f.AccepterId == currentUserId)
                    && f.Status == FriendStatus.Pending);

        // bits in viewbag to make us of inside html
        ViewBag.user = user;
        ViewBag.friends = friends.ToList();
        ViewBag.currentUserId = currentUserId;
        ViewBag.PendingRequests = PendingRequests.ToList();        
        ViewBag.AlreadyFriends = AlreadyFriends;

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

    [Route("/friends/add")]
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
        // this is used to pull information from the headers to redirect you to where you just were
        return Redirect(Request.Headers["Referer"].ToString());
    }
}
