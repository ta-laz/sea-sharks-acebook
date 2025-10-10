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
}
