using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;

namespace Acebook.Tests
{
    public class TheAquariumPlaywright : PageTest
    {
        private const string BaseUrl = "http://127.0.0.1:5287";

        [OneTimeSetUp]
        public async Task OneTime()
        {
            await using var context = new AcebookDbContext();
            await TestDataSeeder.EnsureDbReadyAsync(context);
        }

        [SetUp]
        public async Task SetupDb()
        {
            await using var context = new AcebookDbContext();
            await TestDataSeeder.ResetAndSeedAsync(context);
            // Go to sign-in page
            await Page.GotoAsync("/signin");
            // Wait for form to load
            await Page.WaitForSelectorAsync("#signin-submit", new() { State = WaitForSelectorState.Visible });
            // Fill and submit
            await Page.GetByTestId("email").FillAsync("finn.white@sharkmail.ocean");
            await Page.GetByTestId("password").FillAsync("password123");
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts"),
                Page.GetByTestId("signin-submit").ClickAsync()
            );
        }

        public override BrowserNewContextOptions ContextOptions()
          => new BrowserNewContextOptions
          {
              BaseURL = BaseUrl
          };

        [Test]
        public async Task CreatePost_PostsIndexPage_DisplaysPostOnSamePage()
        {
            // Create post
            await Page.GetByTestId("post-content-input").FillAsync("Test content");
            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.GetByTestId("post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/posts")
            );
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();
        }

        [Test]
        public async Task FriendsButton_TheAquarium_RedirectsToMyFriends()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to the aquarium
            // Click friends to redirect to friends page
            await Task.WhenAll(
                Page.GetByTestId("redirect-friends").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/friends")
            );
        }

        [Test]
        public async Task ViewingFriends_FriendProfilePage_ShowsListOfTheirFriends()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to the aquarium
            // Click name on a post
            await Page.GetByTestId("Shelly").First.ClickAsync();
            // redirects to Shelly's profile page
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/2");
            // Expect Shelly's friends' names to be visible
            await Expect(Page.GetByTestId("Finn")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("Bruce")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("Coral")).ToBeVisibleAsync();
        }

        [Test]
        public async Task LeftSideBar_TheAquarium_ShowsLoggedInUsersName()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to the aquarium
            // Expect Finn White to be visible in the sidebar
            await Expect(Page.GetByTestId("sidebar-username")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("sidebar-username")).ToHaveTextAsync("Finn White");
        }

        [Test]
        public async Task LeftSideBar_TheAquarium_UsersNameRedirectsToProfile()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to the aquarium
            await Page.GetByTestId("sidebar-username").ClickAsync();
            // redirects to Finn's profile page
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
        }

    }
}