using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;
using Acebook.TestHelpers;

namespace Acebook.Tests
{
    public class MyFriendProfilePagePlaywright : PageTest
    {
        private const string BaseUrl = "http://127.0.0.1:5287";

        [OneTimeSetUp]
        public async Task OneTime()
        {
            await using var context = DbFactory.CreateTestDb();
            await TestDataSeeder.EnsureDbReadyAsync(context);
        }

        [SetUp]
        public async Task SetupDb()
        {
            await using var context = DbFactory.CreateTestDb();
            await TestDataSeeder.ResetAndSeedAsync(context);
            // Go to sign-in page
            await Page.GotoAsync("/signin");
            // Wait for form to load
            await Page.WaitForSelectorAsync("#signin-submit", new() { State = WaitForSelectorState.Visible });
            // Fill and submit
            await Page.GetByTestId("email").FillAsync("finn.white@sharkmail.ocean");
            await Page.GetByTestId("password").FillAsync("password123");
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts", new() { Timeout = 4000 }), 
                Page.GetByTestId("signin-submit").ClickAsync()
            );
            // Open profile dropdown
            await Page.WaitForSelectorAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#MyProfile");
            // Wait for profile page to load
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
            // Click Shelly's name in friend's list, redirect to their profile
            await Page.GetByTestId("Friend-link Shelly").First.ClickAsync();
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/2");
        }

        public override BrowserNewContextOptions ContextOptions()
          => new BrowserNewContextOptions
          {
              BaseURL = BaseUrl
          };

        [Test]
        public async Task TaglineAppearsUnderName_OtherProfilePage()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to Shelly's profile page (users/2)
            // Expect the tagline to be the string from test data seeder
            await Expect(Page.GetByTestId("under-name-tagline-text")).ToHaveTextAsync("Predator of productivity, lover of plankton memes.");
        }

        [Test]
        public async Task WriteOnWall_OtherProfilePage_DisplaysPost()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to Shelly's profile page (users/2)
            // Post on Shelly's wall
            await Page.GetByTestId("create-post-input").FillAsync("Test content");
            await Task.WhenAll(
                Page.GetByTestId("create-post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/users/2")
            );
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("post-content").First).ToHaveTextAsync("Test content");
        }

        [Test]
        public async Task FriendNameOnPost_OtherProfilePage_RedirectsToTheirProfile()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to Shelly's profile page (users/2)
            // Click friend's name on post
            await Page.GetByTestId("Post-link Finn").First.ClickAsync();
            // redirects to Finn's profile page
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
        }

        [Test]
        public async Task ViewPage_NotFriendProfilePage_WriteOnWallNotVisible()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to Shelly's profile page (users/2)
            // Go to user that Finn is not friends with
            await Page.GotoAsync("users/4");
            // check that the create post form is not visible
            await Expect(Page.GetByTestId("create-post-input")).ToBeHiddenAsync();
        }

        [Test]
        public async Task SeeAllFriendsButton_OtherProfilePage_RedirectsToFriendsOfOtherUser()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to Shelly's profile page (users/2)
            // Click see all friends to redirect to Shelly's friends page
            await Task.WhenAll(
                Page.GetByTestId("see-all-friends").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/friends/2")
            );
        }

        [Test]
        public async Task FriendsProfilePage_OtherProfilePage_ShowsAlreadyFriends()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to Shelly's profile page (users/2)
            await Expect(Page.GetByTestId("already-friends")).ToBeVisibleAsync();
        }

        [Test]
        public async Task PendingFriendRequest_OtherProfilePage_ShowsFriendRequestSent()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to Shelly's profile page (users/2)
            await Page.GotoAsync("users/4");
            await Expect(Page.GetByTestId("friend-request-sent")).ToBeVisibleAsync();
        }

        [Test]
        public async Task NotFriends_OtherProfilePage_AddFriendButtonIsVisible()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to Shelly's profile page (users/2)
            await Page.GotoAsync("users/6");
            await Expect(Page.GetByTestId("add-friend")).ToBeVisibleAsync();
        }

        [Test]
        public async Task AddFriends_OtherProfilePage_ChangesToShowFriendRequestSent()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to Shelly's profile page (users/2)
            await Page.GotoAsync("users/6");
            Page.Dialog += async (_, dialog) =>
                {
                    Assert.That(dialog.Message, Does.Contain("Are you sure you want to add this person?"));
                    await dialog.AcceptAsync();
                };
            await Page.GetByTestId("add-friend").ClickAsync();
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/6");
            await Expect(Page.GetByTestId("friend-request-sent")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Unfriend_OtherProfilePage_ChangesToAddFriend()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to Shelly's profile page (users/2)
            Page.Dialog += async (_, dialog) =>
                {
                    Assert.That(dialog.Message, Does.Contain("Are you sure"));
                    await dialog.AcceptAsync();
                };
            await Page.GetByTestId("unfriend").ClickAsync();
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/2");
            await Expect(Page.GetByTestId("add-friend")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Unfriend_OtherProfilePage_WriteOnWallNowHidden()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to Shelly's profile page (users/2)
            Page.Dialog += async (_, dialog) =>
                {
                    Assert.That(dialog.Message, Does.Contain("Are you sure"));
                    await dialog.AcceptAsync();
                };
            await Page.GetByTestId("unfriend").ClickAsync();
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/2");
            await Expect(Page.GetByTestId("create-post-input")).ToBeHiddenAsync();
        }

        [Test]
        public async Task CommentButton_OtherProfilePage_NavigatesToPostPage()
        {

            // Click on a post:
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts/7"),
                Page.GetByTestId("comment-button").First.ClickAsync()
            );
            await Expect(Page.Locator("#splash-heading")).ToContainTextAsync("Shelly's Splash");
        }
        
        [Test]
        public async Task SeeMoreButton_OtherProfilePage_NavigatesToPostPage()
        {
            // Click on a post:
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts/7"),
                Page.GetByTestId("see-more-button").First.ClickAsync()
            );
            await Expect(Page.Locator("#splash-heading")).ToContainTextAsync("Shelly's Splash");
        }
    }
}