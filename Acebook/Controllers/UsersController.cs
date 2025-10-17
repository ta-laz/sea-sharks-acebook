using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using System.Security.Cryptography;
using Acebook.ViewModels;
using acebook.ActionFilters;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace acebook.Controllers;

public class UsersController : Controller
{
    private readonly ILogger<UsersController> _logger;
    private readonly IPasswordHasher<User> _hasher;

    public UsersController(ILogger<UsersController> logger, IPasswordHasher<User> hasher)
    {
        _logger = logger;
        _hasher = hasher;
    }

    [Route("/signup")]
    [HttpGet]
    public IActionResult New()
    {
        int? id = HttpContext.Session.GetInt32("user_id");
        if (id != null)
        {
            return Redirect("/posts");
        }
        return View();
    }

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}")]
    [HttpGet]
    public IActionResult Index(int id)
    {
        AcebookDbContext dbContext = new AcebookDbContext();

        // find the user that has the id of the page we're looking at
        // include that user's posts and profile bio details
        var user = dbContext.Users
                  .Include(u => u.ProfileBio)
                  .Include(u => u.Posts)
                  .FirstOrDefault(u => u.Id == id);

        int? currentUserId = HttpContext.Session.GetInt32("user_id");

        if (user == null)
            return NotFound();

        // retrieve all the posts where the wallid matches the id of the page we're on
        var posts = dbContext.Posts.Where(p => p.WallId == id)
                                   .Include(p => p.User)
                                   .Include(p => p.Comments)
                                        .ThenInclude(c => c.Likes)
                                    .Include(p => p.Likes)
                                   .OrderByDescending(p => p.CreatedOn);
        foreach (var post in posts)
        {
            post.UserHasLiked = post.Likes.Any(l => l.UserId == currentUserId);
            if (post.Comments != null)
            {
                foreach (var comment in post.Comments)
                {
                    comment.UserHasLiked = comment.Likes.Any(l => l.UserId == currentUserId);
                }
            }
        }
        ViewBag.Posts = posts.ToList();

        // search through the friends table, filter for records where the requester and accepter have 
        // the id of the page we're on and the status is accepted
        // take up to 3 friends and make it a list to display it on the user's profile page
        var friends = dbContext.Friends
        .Include(f => f.Requester)
        .Include(f => f.Accepter)
        .Where(f => (f.RequesterId == id || f.AccepterId == id) && f.Status == FriendStatus.Accepted)
        .Take(3)
        .ToList();


        ViewBag.CurrentUserId = currentUserId;
        ViewBag.Friends = friends;
        ViewBag.ProfileUserId = id;

        // ✅ Check if logged-in user and viewed user are friends
        bool friendship = dbContext.Friends.Any(f =>
            ((f.RequesterId == currentUserId && f.AccepterId == id) ||
             (f.RequesterId == id && f.AccepterId == currentUserId))
             && f.Status == FriendStatus.Accepted
        );
        ViewBag.Friendship = friendship;

        // ✅ Build list of accepted friendships for the logged-in user
        var acceptedFriendships = dbContext.Friends
            .Where(f => (f.RequesterId == currentUserId || f.AccepterId == currentUserId)
                     && f.Status == FriendStatus.Accepted)
            .ToList();

        var alreadyFriends = new List<int>();
        foreach (var f in acceptedFriendships)
        {
            alreadyFriends.Add(f.RequesterId == currentUserId ? f.AccepterId : f.RequesterId);
        }

        // ✅ Build list of pending requests involving the logged-in user
        var pendingRequests = dbContext.Friends
            .Where(f =>
                (f.RequesterId == currentUserId || f.AccepterId == currentUserId) &&
                f.Status == FriendStatus.Pending)
            .ToList();

        // ✅ Pass both lists to the view
        ViewBag.AlreadyFriends = alreadyFriends;
        ViewBag.PendingRequests = pendingRequests;

        // if the logged in user's id matches the id of the page we're on render the my profile HTML
        if (currentUserId == id)
        {
            return View("MyProfile", user);
        }
        // else render the other profile HTML
        else
        {
            return View("OtherProfile", user);
        }
    }

    [Route("/users")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(SignUpViewModel suvm)
    {
        if (!ModelState.IsValid)
        {
            return View("New", suvm);
        }

        AcebookDbContext dbContext = new AcebookDbContext();
        if (dbContext.Users.Any(user => user.Email == suvm.Email))
        {
            ModelState.AddModelError("", "Email already registered.");
            return View("New", suvm);
        }

        User user = new User
        {
            FirstName = suvm.FirstName,
            LastName = suvm.LastName,
            Email = suvm.Email,
        };

        user.Password = _hasher.HashPassword(user, suvm.Password);

        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        ProfileBio bio = new ProfileBio
        {
            UserId = user.Id,
            DOB = suvm.DOB
        };
        dbContext.ProfileBios.Add(bio);
        dbContext.SaveChanges();

        HttpContext.Session.SetInt32("user_id", user.Id);
        return new RedirectResult("/posts");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}/update")]
    [HttpGet]
    public IActionResult Update(int id)
    {
        AcebookDbContext dbContext = new AcebookDbContext();

        int? userId = HttpContext.Session.GetInt32("user_id");
        if (userId != id)
        {
            var realUser = dbContext.Users
                  .FirstOrDefault(u => u.Id == userId);
            TempData["Sneaky"] = "You can only edit your own bio you sneaky shark!";
            return RedirectToAction("Update", "Users", new { id = userId });
        }

        var user = dbContext.Users
                  .Include(u => u.ProfileBio)
                  .Include(u => u.Posts)
                  .FirstOrDefault(u => u.Id == id);

        if (user == null)
            return NotFound();

        return View(user);
    }

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}/update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int id, string tagline, string relationshipStatus, string pets, string job)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        var profileBio = dbContext.ProfileBios.Find(id);

        profileBio.Tagline = tagline;
        profileBio.RelationshipStatus = relationshipStatus;
        profileBio.Pets = pets;
        profileBio.Job = job;
        dbContext.SaveChanges();

        if (profileBio == null)
            return NotFound();

        return new RedirectResult($"/users/{id}");
    }


    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}/update-account")]
    [HttpGet]
    public IActionResult UpdateAccount(int id)
    {
        AcebookDbContext dbContext = new AcebookDbContext();

        int? userId = HttpContext.Session.GetInt32("user_id");
        if (userId != id)
        {
            var realUser = dbContext.Users
                  .FirstOrDefault(u => u.Id == userId);
            TempData["Sneaky"] = "You can only edit your own account you sneaky shark!";
            return RedirectToAction("UpdateAccount", "Users", new { id = userId });
        }

        var user = dbContext.Users
                  .FirstOrDefault(u => u.Id == id);

        if (user == null)
            return NotFound();

        ViewBag.SuccessMessage = TempData["SuccessMessage"];

        return View(user);
    }

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}/update-account-name")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateAccountName(int id, string firstName, string lastName, string password)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        var user = dbContext.Users.Find(id);

        if (user == null)
            return NotFound();

        var result = _hasher.VerifyHashedPassword(user, user.Password, password);
        if (result == PasswordVerificationResult.Failed)
        {
            ViewBag.NameError = "Password incorrect";
            return View("UpdateAccount", user);
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.Password = _hasher.HashPassword(user, password);
            dbContext.SaveChanges();
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        dbContext.SaveChanges();

        TempData["SuccessMessage"] = $"Name successfully changed!<br> <a class='hover:underline text-teal-700 justify-center' href='/users/{id}'>Go to your profile</a>";
        return RedirectToAction("UpdateAccount", new { id });

    }

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}/update-account-password")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateAccountPassword(int id, ChangePasswordViewModel cpvm)
    {
        AcebookDbContext dbContext = new AcebookDbContext();
        var user = dbContext.Users.Find(id);

        if (!ModelState.IsValid)
        {
            return View("UpdateAccount", user);
        }
        if (user == null)
            return NotFound();

        var verify = _hasher.VerifyHashedPassword(user, user.Password, cpvm.CurrentPassword);
        if (verify == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Incorrect password");
            return View("UpdateAccount", user);
        }

        if (verify == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.Password = _hasher.HashPassword(user, cpvm.CurrentPassword);
            dbContext.SaveChanges();
        }

        // --- Set NEW password (always hash with hasher) ---
        user.Password = _hasher.HashPassword(user, cpvm.NewPassword);
        dbContext.SaveChanges();

        TempData["SuccessMessage"] = $"Password successfully changed!<br>  <a class='underline text-teal-700' href='/users/{id}'>Go to your profile</a>";

        return RedirectToAction("UpdateAccount", new { id });
    }

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/upload-profile-picture")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
    {
        var currentUserId = HttpContext.Session.GetInt32("user_id");

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile_pics");
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profilePicture.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await profilePicture.CopyToAsync(stream);
        }

        var dbContext = new AcebookDbContext();
        var user = dbContext.Users.Find(currentUserId);
        user.ProfilePicturePath = $"/images/profile_pics/{fileName}";
        dbContext.SaveChanges();

        return Redirect($"/users/{currentUserId}");
    }


    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/delete-account")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(string confirmDeletePassword)
    {

        var currentUserId = HttpContext.Session.GetInt32("user_id");

        if (currentUserId is null)
        {
            return RedirectToAction("SignIn", "Sessions");
        }

        AcebookDbContext dbContext = new AcebookDbContext();
        var user = dbContext.Users.Find(currentUserId);

        if (user == null)
            return NotFound();

        var verify = _hasher.VerifyHashedPassword(user, user.Password, confirmDeletePassword);
        if (verify == PasswordVerificationResult.Failed)
        {
            TempData["DeleteError"] = "Unsuccessful - Password incorrect!";
            return RedirectToAction("UpdateAccount", "Users", new { id = currentUserId });
        }

        using var tx = dbContext.Database.BeginTransaction();

        var friendListEntries = dbContext.Friends
            .Where(f => f.RequesterId == currentUserId || f.AccepterId == currentUserId)
            .ToList();
        dbContext.Friends.RemoveRange(friendListEntries);

        dbContext.Users.Remove(user);
        dbContext.SaveChanges();
        tx.Commit();

        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home");
    }

}

    // Legacy hasher
    // private static string HashPassword(string password)
    // {
    //     using var sha256 = SHA256.Create();
    //     var bytes = System.Text.Encoding.UTF8.GetBytes(password);
    //     var hash = sha256.ComputeHash(bytes);
    //     return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    // }