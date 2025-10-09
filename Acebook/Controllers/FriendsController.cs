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
    public IActionResult Index()
    {
        AcebookDbContext dbContext = new AcebookDbContext();

        int? currentUserId = HttpContext.Session.GetInt32("user_id");

        var friends = dbContext.Friends
        .Include(f => f.Requester)
        .Include(f => f.Accepter)
        .Where(f => f.RequesterId == currentUserId || f.AccepterId == currentUserId && f.Status == FriendStatus.Accepted);

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

        return View();
    }

    [HttpPost]
    public IActionResult Unfriend(int friendId)
    {
        AcebookDbContext dbContext = new AcebookDbContext();

        var friend = dbContext.Friends.Find(friendId);
        dbContext.Friends.Remove(friend);
        dbContext.SaveChanges();

        return RedirectToAction("Index");
    }
}
