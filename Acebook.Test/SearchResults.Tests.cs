using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;
using Acebook.TestHelpers;

namespace Acebook.Tests
{
    public class SearchResultsPlaywright : PageTest
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
            await Page.Locator("#email").FillAsync("finn.white@sharkmail.ocean");
            await Page.Locator("#password").FillAsync("password123");
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts", new() { Timeout = 4000 }), 
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
            await Expect(Page.GetByTestId("Chomp-user-search")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("Cobalt-user-search")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("Coral-user-search")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("post-search-Bluey").First).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("comment-search-Harbor").First).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("comment-search-Cerulean").First).ToBeVisibleAsync();

        }
        [Test]
        public async Task CheckThatPostsCanBeExpandedIntoMore()
        {
            await Page.Locator("#dropdown-button").ClickAsync();
            await Page.GetByTestId("post-test-button").ClickAsync();
            await Page.Locator("#search-input").FillAsync("Finn");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test1")).ToBeVisibleAsync();
            await Page.GetByTestId("showing-all-results-test1").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test1")).ToHaveTextAsync("Show Less");
        }
        [Test]
        public async Task CheckThatPeopleCanBeExpandedIntoMore()
        {
            await Page.Locator("#dropdown-button").ClickAsync();
            await Page.GetByTestId("user-test-button").ClickAsync();
            await Page.Locator("#search-input").FillAsync("Reef");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test2")).ToBeVisibleAsync();
            await Page.GetByTestId("showing-all-results-test2").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test2")).ToHaveTextAsync("Show Less");
        }
        [Test]
        public async Task CheckThatCommentsCanBeExpandedIntoMore()
        {
            await Page.Locator("#dropdown-button").ClickAsync();
            await Page.GetByTestId("comment-test-button").ClickAsync();
            await Page.Locator("#search-input").FillAsync("Reef");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test3")).ToBeVisibleAsync();
            await Page.GetByTestId("showing-all-results-test3").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test3")).ToHaveTextAsync("Show Less");
        }
        [Test]
        public async Task CheckThatdefualtCanBeExpandedIntoMore()
        {
            await Page.Locator("#dropdown-button").ClickAsync();
            //await Page.GetByTestId("comment-test-button").ClickAsync();
            await Page.Locator("#search-input").FillAsync("Reef");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test1")).ToBeVisibleAsync();
            await Page.GetByTestId("showing-all-results-test1").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test1")).ToHaveTextAsync("Show Less");
            await Expect(Page.GetByTestId("showing-all-results-test2")).ToBeVisibleAsync();
            await Page.GetByTestId("showing-all-results-test2").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test2")).ToHaveTextAsync("Show Less");
            await Expect(Page.GetByTestId("showing-all-results-test3")).ToBeVisibleAsync();
            await Page.GetByTestId("showing-all-results-test3").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test3")).ToHaveTextAsync("Show Less");
        }
        [Test]
        public async Task CheckThatPostsIfExpandedDisplayResults()
        {
            await Page.Locator("#dropdown-button").ClickAsync();
            await Page.GetByTestId("post-test-button").ClickAsync();
            await Page.Locator("#search-input").FillAsync("Finn");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test1")).ToBeVisibleAsync();
            await Page.GetByTestId("showing-all-results-test1").ClickAsync();
            var tag = Page.Locator("[data-testid='content-test-for-post']");
            await Expect(tag).ToHaveCountAsync(2);
        }
        [Test]
        public async Task CheckThatUserIfExpandedDisplayResults()
        {
            await Page.Locator("#dropdown-button").ClickAsync();
            await Page.GetByTestId("user-test-button").ClickAsync();
            await Page.Locator("#search-input").FillAsync("Reef");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test2")).ToBeVisibleAsync();
            await Page.GetByTestId("showing-all-results-test2").ClickAsync();
            var tag = Page.Locator("#friend-name");//they cant see @user.FirstName data-testid's probs cus the test cant access the firstname from the db
            await Expect(tag).ToHaveCountAsync(7);// so i did it for the id which isnt skipped so it counts. them all this time
        }
        [Test]
        public async Task CheckThatCommentIfExpandedDisplayResults()
        {
            await Page.Locator("#dropdown-button").ClickAsync();
            await Page.GetByTestId("comment-test-button").ClickAsync();
            await Page.Locator("#search-input").FillAsync("Reef");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByTestId("showing-all-results-test3")).ToBeVisibleAsync();
            await Page.GetByTestId("showing-all-results-test3").ClickAsync();
            var tag = Page.Locator("[data-testid='comment-expanded-saw-more-comments']");
            await Expect(tag).ToHaveCountAsync(45);
        }

        [Test]
        public async Task SearchForFriend_SearchResultsPage_ShowsAlreadyFriends()
        {
            await Page.Locator("#dropdown-button").ClickAsync();
            await Page.GetByTestId("user-test-button").ClickAsync();
            await Page.Locator("#search-input").FillAsync("Shelly");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByTestId("already-friends")).ToBeVisibleAsync();
        }

        [Test]
        public async Task SearchForUserPendingRequest_SearchResultsPage_ShowsPending()
        {
            await Page.Locator("#dropdown-button").ClickAsync();
            await Page.GetByTestId("user-test-button").ClickAsync();
            await Page.Locator("#search-input").FillAsync("Coral");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByTestId("friend-request-sent")).ToBeVisibleAsync();
        }

        [Test]
        public async Task SearchForUserNotAFriend_SearchResultsPage_ShowsAddFriend()
        {
            await Page.Locator("#dropdown-button").ClickAsync();
            await Page.GetByTestId("user-test-button").ClickAsync();
            await Page.Locator("#search-input").FillAsync("Tigra");
            await Page.GetByTestId("search-submit").ClickAsync();
            await Expect(Page.GetByTestId("add-friend")).ToBeVisibleAsync();
        }
    }
}