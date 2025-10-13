using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models; 
using Acebook.Test;

namespace Acebook.Tests
{
    public class MyFriendProfilePagePlaywright : PageTest
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
            // Open profile dropdown
            await Page.WaitForSelectorAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#MyProfile");
            // Wait for profile page to load
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
            // Click Shelly's name in friend's list, redirect to their profile
            await Page.GetByTestId("Shelly").First.ClickAsync();
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

    
    }
}