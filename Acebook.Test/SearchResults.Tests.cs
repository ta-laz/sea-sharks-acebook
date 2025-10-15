using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;

namespace Acebook.Tests
{
    public class SearchResultsPlaywright : PageTest
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
        public async Task CheckSearchFunctionSearches()
        {
            await Page.Locator("#search-input").FillAsync("Reef");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByText("Search Results for: Reef")).ToBeVisibleAsync();
        }
        [Test]
        public async Task NoSearchResultsShowsNoResultsMessage()
        {
            await Page.Locator("#search-input").FillAsync("Test");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByText("No results found for: test")).ToBeVisibleAsync();
        }
        [Test]
        public async Task AllResultsShowResultsForUsersPostsAndComments()
        {
            await Page.Locator("#search-input").FillAsync("Reef");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByTestId("user-results")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("post-results")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("comment-results")).ToBeVisibleAsync();
        }
        [Test]
        public async Task CheckCorrectResultsShowInAll()
        {
            await Page.Locator("#search-input").FillAsync("Reef");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByTestId("user-results")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("post-results")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("comment-results")).ToBeVisibleAsync();
        }
    }
}