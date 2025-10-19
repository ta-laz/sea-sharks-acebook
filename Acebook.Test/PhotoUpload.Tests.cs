using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;
using System.Text.RegularExpressions;
using Acebook.TestHelpers;


namespace Acebook.Tests
{
    public class PhotoUpload : PageTest
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
            SetDefaultExpectTimeout(1000);
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
        public async Task CanUploadImage_OnAquarium()
        {
            await Page.GetByTestId("post-content-input").FillAsync("Test content");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "testImage.jpg");
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
            var path = Path.Combine(Directory.GetCurrentDirectory(), "testImage.jpg");
            await Page.SetInputFilesAsync("#file-upload", path);

            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/users/2")
            );
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("post-picture-176")).ToBeVisibleAsync();
        }

        [Test]
        public async Task CreatePost_EmptyContent_ShowsBrowserValidation_onAquarium()
        {
            await Page.GotoAsync("/posts");


           await Page.Locator("#post-submit").ClickAsync();

            var isValid = await Page.GetByTestId("post-content-input").EvaluateAsync<bool>("el => el.checkValidity()");
            Assert.That(isValid, Is.False, "Expected the textarea to be invalid due to 'required' attribute");

            var validationMessage = await Page.GetByTestId("post-content-input").EvaluateAsync<string>("el => el.validationMessage");
            Console.WriteLine(validationMessage);
            Assert.That(validationMessage, Does.Contain("Please fill out this field."));
        }

        [Test]
        public async Task CreatePost_EmptyContent_ShowsBrowserValidation_onMyProfile()
        {
            await Page.GotoAsync("/users/1");


            await Page.Locator("#post-submit").ClickAsync();

            var isValid = await Page.GetByTestId("create-post-input").EvaluateAsync<bool>("el => el.checkValidity()");
            Assert.That(isValid, Is.False, "Expected the textarea to be invalid due to 'required' attribute");

            var validationMessage = await Page.GetByTestId("create-post-input").EvaluateAsync<string>("el => el.validationMessage");
            Console.WriteLine(validationMessage);
            Assert.That(validationMessage, Does.Contain("Please fill out this field."));
        }
        
        [Test]
        public async Task CreatePost_EmptyContent_ShowsBrowserValidation_onFriendsProfile()
        {
            await Page.GotoAsync("/users/2");

            
            await Page.Locator("#post-submit").ClickAsync();

            var isValid = await Page.GetByTestId("create-post-input").EvaluateAsync<bool>("el => el.checkValidity()");
            Assert.That(isValid, Is.False, "Expected the textarea to be invalid due to 'required' attribute");

            var validationMessage = await Page.GetByTestId("create-post-input").EvaluateAsync<string>("el => el.validationMessage");
            Console.WriteLine(validationMessage);
            Assert.That(validationMessage, Does.Contain("Please fill out this field."));
        }

    }
}