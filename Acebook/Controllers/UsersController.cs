using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
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
    private readonly AcebookDbContext _db;

    public UsersController(ILogger<UsersController> logger, IPasswordHasher<User> hasher, AcebookDbContext db)
    {
        _logger = logger;
        _hasher = hasher;
        _db = db;
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
    public async Task<IActionResult> Index(int id)
    {
        // find the user that has the id of the page we're looking at
        // include that user's posts and profile bio details
        var user = await _db.Users
                  .Include(u => u.ProfileBio)
                  .Include(u => u.Posts)
                  .FirstOrDefaultAsync(u => u.Id == id);

        int? currentUserId = HttpContext.Session.GetInt32("user_id");

        if (user == null)
            return NotFound();

        // retrieve all the posts where the wallid matches the id of the page we're on
        var posts = await _db.Posts
            .Where(p => p.WallId == id)
            .Include(p => p.User)
            .Include(p => p.Comments)!.ThenInclude(c => c.Likes)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.CreatedOn)
            .ToListAsync();

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
        ViewBag.Posts = posts;

        // search through the friends table, filter for records where the requester and accepter have 
        // the id of the page we're on and the status is accepted
        // take up to 3 friends and make it a list to display it on the user's profile page
        var friends = await _db.Friends
            .Include(f => f.Requester)
            .Include(f => f.Accepter)
            .Where(f => (f.RequesterId == id || f.AccepterId == id) && f.Status == FriendStatus.Accepted)
            .Take(3)
            .ToListAsync();



        ViewBag.CurrentUserId = currentUserId;
        ViewBag.Friends = friends;
        ViewBag.ProfileUserId = id;

        // ✅ Check if logged-in user and viewed user are friends
        bool friendship = await _db.Friends.AnyAsync(f =>
            ((f.RequesterId == currentUserId && f.AccepterId == id) ||
             (f.RequesterId == id && f.AccepterId == currentUserId)) &&
            f.Status == FriendStatus.Accepted);
        ViewBag.Friendship = friendship;

        // ✅ Build list of accepted friendships for the logged-in user
        var acceptedFriendships = await _db.Friends
            .Where(f => (f.RequesterId == currentUserId || f.AccepterId == currentUserId) &&
                        f.Status == FriendStatus.Accepted)
            .ToListAsync();

        var alreadyFriends = new List<int>();
        foreach (var f in acceptedFriendships)
        {
            alreadyFriends.Add(f.RequesterId == currentUserId ? f.AccepterId : f.RequesterId);
        }

        // ✅ Build list of pending requests involving the logged-in user
        var pendingRequests = await _db.Friends
            .Where(f => (f.RequesterId == currentUserId || f.AccepterId == currentUserId) &&
                        f.Status == FriendStatus.Pending)
            .ToListAsync();

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
    public async Task<IActionResult> Create(SignUpViewModel suvm)
    {
        if (!ModelState.IsValid)
        {
            return View("New", suvm);
        }

        if (await _db.Users.AnyAsync(u => u.Email == suvm.Email))
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

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        ProfileBio bio = new ProfileBio
        {
            UserId = user.Id,
            DOB = suvm.DOB
        };
        _db.ProfileBios.Add(bio);
        await _db.SaveChangesAsync();

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
    public async Task<IActionResult> Update(int id)
    {

        int? userId = HttpContext.Session.GetInt32("user_id");
        if (userId != id)
        {
            var realUser = await _db.Users
                  .FirstOrDefaultAsync(u => u.Id == userId);
            TempData["Sneaky"] = "You can only edit your own bio you sneaky shark!";
            return RedirectToAction("Update", "Users", new { id = userId });
        }

        var user = await _db.Users
                  .Include(u => u.ProfileBio)
                  .Include(u => u.Posts)
                  .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        return View(user);
    }

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}/update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string tagline, string relationshipStatus, string pets, string job)
    {
        var profileBio = await _db.ProfileBios.FindAsync(id);
        if (profileBio == null)
            return NotFound();
        
        profileBio.Tagline = tagline;
        profileBio.RelationshipStatus = relationshipStatus;
        profileBio.Pets = pets;
        profileBio.Job = job;
        await _db.SaveChangesAsync();

        return new RedirectResult($"/users/{id}");
    }


    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}/update-account")]
    [HttpGet]
    public async Task<IActionResult> UpdateAccount(int id)
    {

        int? userId = HttpContext.Session.GetInt32("user_id");
        if (userId != id)
        {
            var realUser = await _db.Users
                  .FirstOrDefaultAsync(u => u.Id == userId);
            TempData["Sneaky"] = "You can only edit your own account you sneaky shark!";
            return RedirectToAction("UpdateAccount", "Users", new { id = userId });
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        ViewBag.SuccessMessage = TempData["SuccessMessage"];

        return View(user);
    }

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}/update-account-name")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAccountName(int id, string firstName, string lastName, string password)
    {
        var user = await _db.Users.FindAsync(id);

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
            await _db.SaveChangesAsync();
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Name successfully changed!<br> <a class='hover:underline text-teal-700 justify-center' href='/users/{id}'>Go to your profile</a>";
        return RedirectToAction("UpdateAccount", new { id });

    }

    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/{id}/update-account-password")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAccountPassword(int id, ChangePasswordViewModel cpvm)
    {
        var user = await _db.Users.FindAsync(id);

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
            await _db.SaveChangesAsync();
        }

        // --- Set NEW password (always hash with hasher) ---
        user.Password = _hasher.HashPassword(user, cpvm.NewPassword);
        await _db.SaveChangesAsync();

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
        if (currentUserId is null) return RedirectToAction("New", "Sessions");

        if (profilePicture == null || profilePicture.Length == 0)
        {
            TempData["UploadError"] = "Please choose an image to upload.";
            return Redirect($"/users/{currentUserId}");
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profile_pics");
        Directory.CreateDirectory(uploadsFolder); // ensures folder exists

        var fileName = Guid.NewGuid() + Path.GetExtension(profilePicture.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await profilePicture.CopyToAsync(stream);
        }

        var user = await _db.Users.FindAsync(currentUserId);
        if (user == null) return NotFound();

        user.ProfilePicturePath = $"/images/profile_pics/{fileName}";
        HttpContext.Session.SetString("user_profile_picture", user.ProfilePicturePath ?? "");
        await _db.SaveChangesAsync();

        return Redirect($"/users/{currentUserId}");
    }


    [ServiceFilter(typeof(AuthenticationFilter))]
    [Route("/users/delete-account")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string confirmDeletePassword)
    {

        var currentUserId = HttpContext.Session.GetInt32("user_id");

        if (currentUserId is null)
        {
            return RedirectToAction("SignIn", "Sessions");
        }

        var user = await _db.Users.FindAsync(currentUserId);

        if (user == null)
            return NotFound();

        var verify = _hasher.VerifyHashedPassword(user, user.Password, confirmDeletePassword);
        if (verify == PasswordVerificationResult.Failed)
        {
            TempData["DeleteError"] = "Unsuccessful - Password incorrect!";
            return RedirectToAction("UpdateAccount", "Users", new { id = currentUserId });
        }
        var strategy = _db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var sentNotes = await _db.Notifications
            .Where(n => n.SenderId == currentUserId)
            .ToListAsync();
        _db.Notifications.RemoveRange(sentNotes);

        var friendListEntries = await _db.Friends
            .Where(f => f.RequesterId == currentUserId || f.AccepterId == currentUserId)
            .ToListAsync();
        _db.Friends.RemoveRange(friendListEntries);

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        });
        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home");
    }

}

