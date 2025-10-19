using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;
using Acebook.TestHelpers;

namespace Acebook.Tests
{
    public class MyUserProfilePagePlaywright : PageTest
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
            await Page.GetByTestId("email").FillAsync("finn.white@sharkmail.ocean");
            await Page.GetByTestId("password").FillAsync("password123");
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts", new() { Timeout = 4000 }), 
                Page.GetByTestId("signin-submit").ClickAsync()
            );
            // Open profile dropdown
            await Page.WaitForSelectorAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#MyProfile");
            // Wait for profile page to load
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
        }

        public override BrowserNewContextOptions ContextOptions()
          => new BrowserNewContextOptions
          {
              BaseURL = BaseUrl
          };

        [Test]
        public async Task CreatePost_MyUserProfilePage_DisplaysPostOnSamePage()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
            // Create post
            await Page.Locator("#create-post-input").FillAsync("Test content");
            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.GetByTestId("create-post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/users/1")
            );
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();
        }

        [Test]
        public async Task TaglineAppearsUnderName_MyUserProfilePage()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
            // Expect the tagline to be the string from test data seeder
            await Expect(Page.GetByTestId("under-name-tagline-text")).ToHaveTextAsync("Explorer of coral reefs and deep thinker.");
        }

        [Test]
        public async Task EditTagline_MyUserProfilePage_UpdatesTagline()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
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
        public async Task CancelEditTagline_MyUserProfilePage_HidesForm()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
            // Click edit tagline to make form appear 
            await Page.ClickAsync("#update-tagline");
            await Page.ClickAsync("#cancel-edit");
            // Expect the tagline to be unchanged
            await Expect(Page.GetByTestId("under-name-tagline-text")).ToHaveTextAsync("Explorer of coral reefs and deep thinker.");
            await Expect(Page.GetByTestId("tagline-form")).ToBeHiddenAsync();
        }


        [Test]
        public async Task ProfileBioShowsDetails_MyUserProfilePage()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
            // Expect the profile bio to display all the data from the test data seeder
            await Expect(Page.GetByTestId("bio-name")).ToHaveTextAsync("Finn's Bio");
            await Expect(Page.GetByTestId("bio-age")).ToHaveTextAsync("Age: 32");
            await Expect(Page.GetByTestId("bio-tagline")).ToHaveTextAsync("Explorer of coral reefs and deep thinker.");
            await Expect(Page.GetByTestId("bio-relationshipstatus")).ToHaveTextAsync("Relationship Status: Single");
            await Expect(Page.GetByTestId("bio-pets")).ToHaveTextAsync("Pets: Remora");
            await Expect(Page.GetByTestId("bio-job")).ToHaveTextAsync("Job: Reef Guide");
        }
        [Test]
        public async Task EditBio_MyUserProfilePage_UpdatesProfileBioRedirectsToUserProfile()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
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
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
            // Click edit bio to redirect to update page
            await Page.ClickAsync("#edit-bio");
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1/update");
            await Page.ClickAsync("#cancel");
            // Wait for profile page to load
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
        }

        [Test]
        public async Task SeeAllFriendsButton_MyUserProfilePage_RedirectsToFriends()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
            // Click see all friends to redirect to friends page
            await Task.WhenAll(
                Page.Locator("#see-all-friends").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/friends")
            );
        }

        [Test]
        public async Task FriendsBlockDisplaysFirstThreeFriends_MyUserProfilePage()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
            // Click see all friends to redirect to friends page
            await Expect(Page.GetByTestId("friends-header")).ToHaveTextAsync("Fronds");
            // each friend get its own test id assigned dynamically by its first name
            await Expect(Page.GetByTestId("Friend-link Shelly")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("Friend-link Bruce")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("Friend-link Reefy")).ToBeVisibleAsync();
        }

        [Test]
        public async Task FriendNameInFriendsBlock_MyUserProfilePage_RedirectsToTheirProfile()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
            // Click friend's name
            await Page.GetByTestId("Friend-link Shelly").ClickAsync();
            // redirects to Shelly's profile page
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/2");
        }

        [Test]
        public async Task FriendNameOnPost_MyUserProfilePage_RedirectsToTheirProfile()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
            // Click friend's name on post
            await Page.GetByTestId("Post-link Bluey").First.ClickAsync();
            // redirects to Bluey's profile page
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/50");
        }

        [Test]
        public async Task ViewingFriends_FriendProfilePage_ShowsListOfTheirFriends()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
            // Click friend's name
            await Page.GetByTestId("Friend-link Shelly").ClickAsync();
            // redirects to Shelly's profile page
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/2");
            // Expect Shelly's friends' names to be visible
            await Expect(Page.GetByTestId("Finn")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("Bruce")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("Coral")).ToBeVisibleAsync();
        }

        [Test]
        public async Task CommentButton_MyProfilePage_NavigatesToPostPage()
        {

            // Click on a post:
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts/174"),
                Page.GetByTestId("comment-button").First.ClickAsync()
            );
            await Expect(Page.Locator("#splash-heading")).ToContainTextAsync("Bluey's Splash");
        }

        [Test]
        public async Task SeeMoreButton_MyProfilePage_NavigatesToPostPage()
        {
            // Click on a post:
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts/174"),
                Page.GetByTestId("see-more-button").First.ClickAsync()
            );
            await Expect(Page.Locator("#splash-heading")).ToContainTextAsync("Bluey's Splash");
        }

        [Test]
        public async Task ViewingFriends_FriendProfilePage_ShowsTheirFirstPost()
        {
            // NOTE: [SetUp] signs in with user Finn then goes to their user profile page (users/1)
            // Click friend's name
            await Page.GetByTestId("Friend-link Shelly").ClickAsync();
            // redirects to Shelly's profile page
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/2");
            await Expect(Page.GetByText("Sardine school shimmering like stars beneath the waves. swim fin swim fin deep blue reef hunt glide kelp wave tide current school")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("post-content").First).ToHaveTextAsync("Sardine school shimmering like stars beneath the waves. swim fin swim fin deep blue reef hunt glide kelp wave tide current school");
        }
        [Test]
        [Obsolete]
        public async Task OnProfilePage_ChangeProfilePicture()
        {
            var avatar = Page.Locator("#profile-pic");
            await Expect(avatar).ToBeVisibleAsync();
            var oldSrc = await avatar.GetAttributeAsync("src");
            await Page.GetByTestId("changing-profile-picture-testing").ClickAsync();
            var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "testImage.jpg");
            Console.WriteLine("File path: " + filePath);
            Assert.That(File.Exists(filePath), "Test image not found!");
            await Page.RunAndWaitForNavigationAsync(async () =>
            {
                await Page.SetInputFilesAsync("[data-testid='changing-profile-picture-testing']", filePath);
            });
            // 4) After navigation, the avatar should be updated
            avatar = Page.Locator("#profile-pic");
            await Expect(avatar).ToBeVisibleAsync();
            var newSrc = await avatar.GetAttributeAsync("src");
            Assert.That(newSrc, Is.Not.Null.And.Not.Empty, "Avatar src should not be empty after upload.");
            const string placeholder = "/images/Placeholder.png";
            if (oldSrc == placeholder)
            {
                // If we started with the placeholder, ensure it changed
                Assert.That(newSrc, Is.Not.EqualTo(placeholder), "Avatar still points to the placeholder after upload.");
            }
            else
            {
                // Otherwise ensure the src actually changed (best if server adds cache-busting query string)
                Assert.That(newSrc, Is.Not.EqualTo(oldSrc),
                    "Avatar src did not change after upload. Consider adding a cache-busting query string server-side.");
            }
        }
    }
}