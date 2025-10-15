using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;
using System.Text.RegularExpressions;


namespace Acebook.Tests
{
    public class PhotoUpload : PageTest
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
        }

        public override BrowserNewContextOptions ContextOptions()
            => new BrowserNewContextOptions
            {
                BaseURL = BaseUrl
            };

        [Test]
        public async Task CanUploadImage_OnAquarium()
        {
            await Page.Locator("#post-content").FillAsync("Test content");
            var path = Path.Combine(Directory.GetCurrentDirectory(),"testImage.jpg");
            await Page.SetInputFilesAsync("#file-upload", path);

            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/posts")
            );
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("post-picture-176")).ToBeVisibleAsync();
        }

        [Test]
        public async Task CanUploadImage_OnMyProfile()
        {
            await Page.GotoAsync("/users/1");
            await Page.GetByTestId("create-post-input").FillAsync("Test content");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "testImage.jpg");
            await Page.SetInputFilesAsync("#file-upload", path);

            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/users/1")
            );
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("post-picture-176")).ToBeVisibleAsync();
        }
        
                [Test]
        public async Task CanUploadImage_OnFriendsProfile()
        {
            await Page.GotoAsync("/users/2");
            await Page.GetByTestId("create-post-input").FillAsync("Test content");
            var path = Path.Combine(Directory.GetCurrentDirectory(),"testImage.jpg");
            await Page.SetInputFilesAsync("#file-upload", path);

            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/users/2")
            );
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("post-picture-176")).ToBeVisibleAsync();
        }

    }
}