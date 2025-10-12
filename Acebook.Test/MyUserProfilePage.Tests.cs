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
        public async Task CreatePost_MyUserProfilePage_DisplaysPostOnSamePage()
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
                Page.GetByTestId("signin-submit").ClickAsync()
            );
            // Open profile dropdown
            await Page.WaitForSelectorAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#MyProfile");
            // Wait for profile page to load
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
            // Create post
            await Page.Locator("#post-content").FillAsync("Test content");
            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.GetByTestId("post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/users/1")
            );
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();
        }

        [Test]
        public async Task EditTagline_MyUserProfilePage_UpdatesTagline()
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
                Page.GetByTestId("signin-submit").ClickAsync()
            );
            // Open profile dropdown
            await Page.WaitForSelectorAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#MyProfile");
            // Wait for profile page to load
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
            // Click edit tagline to make form appear + fill out form
            await Task.WhenAll(
                Page.ClickAsync("#update-tagline"),
                Page.Locator("#tagline-input").FillAsync("Test content")
            );
            // Wait for tagline submission + redirect
            await Task.WhenAll(
                Page.GetByTestId("tagline-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/users/1")
            );
            // Expect the tagline to be the new string
            await Expect(Page.GetByTestId("under-name-tagline-text")).ToHaveTextAsync("Test content");
        }

        [Test]
        public async Task EditBio_MyUserProfilePage_UpdatesProfileBioRedirectsToUserProfile()
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
                Page.GetByTestId("signin-submit").ClickAsync()
            );
            // Open profile dropdown
            await Page.WaitForSelectorAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#MyProfile");
            // Wait for profile page to load
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
            // Click edit bio to redirect to update page
            await Page.ClickAsync("#edit-bio");
            // Wait for update page to load
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1/update");
            // Fill out form
            await Page.Locator("#tagline").FillAsync("Test tagline");
            await Page.Locator("#relationshipstatus").FillAsync("Test status");
            await Page.Locator("#pets").FillAsync("Test pets");
            await Page.Locator("#job").FillAsync("Test job");
            // // Wait for tagline submission + redirect
            await Task.WhenAll(
                Page.Locator("#update-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/users/1")
            );
            // Expect the tagline to be the new string
            await Expect(Page.GetByTestId("bio-tagline")).ToHaveTextAsync("Test tagline");
        }

        [Test]
        public async Task EditBioCancelButton_MyUserProfilePage_GoesBackToUserProfile()
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
                Page.GetByTestId("signin-submit").ClickAsync()
            );
            // Open profile dropdown
            await Page.WaitForSelectorAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#MyProfile");
            // Wait for profile page to load
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
            // Click edit bio to redirect to update page
            await Page.ClickAsync("#edit-bio");
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1/update");
            await Page.ClickAsync("#cancel");
            // Wait for profile page to load
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
            
        }

    }
}