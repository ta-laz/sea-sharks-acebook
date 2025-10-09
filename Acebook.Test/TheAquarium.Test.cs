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
        }

        public override BrowserNewContextOptions ContextOptions()
          => new BrowserNewContextOptions
          {
              BaseURL = BaseUrl
          };

        [Test]
        public async Task CreatePost_PostsIndexPage_DisplaysPostOnSamePage()
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
            // Create post
            await Page.Locator("#post-content").FillAsync("Test content");
            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/posts")
            );
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();
        }

    }
}