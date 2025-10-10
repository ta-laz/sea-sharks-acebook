using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;
using System.Data.Common;

namespace Acebook.Tests
{
    public class FriendListPageTests : PageTest
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
        }

        public override BrowserNewContextOptions ContextOptions()
          => new BrowserNewContextOptions
          {
              BaseURL = BaseUrl
          };

        // 1. Check if the page loads - go to the URL /friends and check if My Friends can be found and see if friend list is visibile 
        // 2. If friend requests clicked, are my friend requests visibile 
        // 3. If sent friend requests clicked, are they visible 
        // 4. if search bar Shelley, check to see if shelley pops up

        [Test]
        public async Task FriendListPage_GoToURL_DisplaysFriendList()
        {
            // Go to sign-in page
            SetDefaultExpectTimeout(1000);
            await Page.GotoAsync("/signin");
            // Wait for form to load
            await Page.WaitForSelectorAsync("#signin-submit", new() { State = WaitForSelectorState.Visible });
            // Fill and submit
            await Page.Locator("#email").FillAsync("finn.white@sharkmail.ocean");
            await Page.Locator("#password").FillAsync("password123");
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts"),
                Page.Locator("#signin-submit").ClickAsync()
            );

            // Go to Friends URL
            await Page.GotoAsync("/friends");
            await Expect(Page.GetByText("My Friends")).ToBeVisibleAsync();
            await Expect(Page.GetByText("Friend List")).ToBeVisibleAsync();

        }


    }
}