using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models; 
using Acebook.Test;

namespace Acebook.Tests
{
    public class MyUserProfilePagePlaywright : PageTest
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
        public async Task X()
        {
            await Page.GotoAsync("/signin");

            await Page.Locator("#email").FillAsync("finn.white@sharkmail.ocean");
            await Page.Locator("#password").FillAsync("password123");
            await Page.Locator("#submit").ClickAsync();

            await Page.ClickAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#MyProfile");

            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
        }
    }
}