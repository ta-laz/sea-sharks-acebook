using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;
using Acebook.TestHelpers;

namespace Acebook.Tests
{
    public class NewUserPlaywright : PageTest
    {
        private const string BaseUrl = "http://127.0.0.1:5287";

        [OneTimeSetUp]
        public async Task OneTime()
        {
            await using var context = DbFactory.CreateTestDb();
            await TestDataSeeder.EnsureDbReadyAsync(context);
        }
        public override BrowserNewContextOptions ContextOptions()
          => new BrowserNewContextOptions
        {
            BaseURL = BaseUrl
        };
        [Test]
        [Obsolete]
        public async Task OnNewUser_TheyHaveProfilePicture()
        {
            await using var context = DbFactory.CreateTestDb();
            await TestDataSeeder.ResetAndSeedAsync(context);
            await Page.GotoAsync("/signup");
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/signup");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Page.Locator("#firstname").FillAsync("Francine");
            await Page.Locator("#lastname").FillAsync("Gills");
            await Page.Locator("#dob").FillAsync("1995-08-10");
            await Page.Locator("#email").FillAsync("e@sharkmail.ocean");
            await Page.Locator("#password").FillAsync("Password123!");
            await Page.Locator("#confirmpassword").FillAsync("Password123!");

            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts", new() { Timeout = 4000 }),
                Page.Locator("#submit").ClickAsync()
            );
            
            // Open profile dropdown
            await Page.WaitForSelectorAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#MyProfile");
            // Wait for profile page to load
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/51");// the number at the end depends on how many users are in the db
            var avatar = Page.Locator("#profile-pic");
            await Expect(avatar).ToBeVisibleAsync();
        }
    }
}