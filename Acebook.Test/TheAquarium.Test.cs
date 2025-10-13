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
            await Page.Locator("#email").FillAsync("finn.white@sharkmail.ocean");
            await Page.Locator("#password").FillAsync("password123");
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
            await Page.Locator("#post-content").FillAsync("Test content");
            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/posts")
            );
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();
        }
        // [Test]
        // public async Task NumberOfLikesShowsBeneathEachPost()
        // {
        //     SetDefaultExpectTimeout(1000);
        //     await Page.GotoAsync("/signin");
        //     await Page.WaitForSelectorAsync("#signin-submit", new() { State = WaitForSelectorState.Visible });
        //     await Page.Locator("#email").FillAsync("finn.white@sharkmail.ocean");
        //     await Page.Locator("#password").FillAsync("password123");
        //     await Task.WhenAll(
        //         Page.WaitForURLAsync($"{BaseUrl}/posts"),
        //         Page.Locator("#signin-submit").ClickAsync()
        //     );
        //     await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();
        // }

        [Test]
        public async Task SeeAllFriendsButton_MyUserProfilePage_RedirectsToFriends()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
            // Click see all friends to redirect to friends page
            await Task.WhenAll(
                Page.Locator("#redirect-friends").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/friends")
            );
        }

    }
}