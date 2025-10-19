using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;
using System.Data.Common;
using Acebook.TestHelpers;

namespace Acebook.Tests
{
    public class FriendListPageTests : PageTest
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
            await Page.GotoAsync("/friends");
        }

        public override BrowserNewContextOptions ContextOptions()
            => new BrowserNewContextOptions
            {
                BaseURL = BaseUrl
            };

        // 1. Check if the page loads - go to the URL /friends and check if My Friends can be found and see if friend list is visibile  - DONE
        // 2. If friend requests clicked, are my friend requests visibile - DONE
        // 3. If sent friend requests clicked, are they visible - DONE
        // 4. if search bar Shelley, check to see if shelley pops up - DONE 

        [Test]
        public async Task FriendListPage_GoToURL_DisplaysFriendList()
        {
            // NOTE: each test is set up as signed in with Finn and goes to /friends
            await Expect(Page.GetByTestId("myFriendsPageTitle")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("friends-list-subheader")).ToBeVisibleAsync();

        }

        [Test]
        public async Task FriendListPage_ClickMyFriendRequests_DisplaysFriendRequests()
        {
            // NOTE: each test is set up as signed in with Finn and goes to /friends
            await Page.ClickAsync("#received-label");
            await Expect(Page.GetByText("Received Requests")).ToBeVisibleAsync();
        }

        [Test]
        public async Task FriendListPage_ClickSentRequests_DisplaysSentFriendRequests()
        {
            // NOTE: each test is set up as signed in with Finn and goes to /friends
            await Page.GetByTestId("sent-requests-label").ClickAsync();
            await Expect(Page.GetByTestId("sent-requests-label")).ToBeVisibleAsync();
        }

        [Test]
        public async Task FriendListPage_SearchForShelly_DisplaysShelly()
        {
            // NOTE: each test is set up as signed in with Finn and goes to /friends
            await Page.Locator("input[name='SearchQuery']").FillAsync("Shelly");
            await Page.GetByRole(AriaRole.Main)
                .GetByRole(AriaRole.Button, new() { Name = "Search" })
                .ClickAsync();

            // await Page.Keyboard.PressAsync("Enter");

            await Expect(Page.GetByText("Shelly")).ToBeVisibleAsync();
        }

        [Test]
        public async Task FriendListPage_ClickAcceptHarbor_DisplaysHarborInFriendList()
        {
            // NOTE: each test is set up as signed in with Finn and goes to /friends
            await Page.GetByText("Harbor")
                .Locator("..")
                .GetByRole(AriaRole.Button, new() { Name = "Accept" })
                .ClickAsync();

            await Page.WaitForURLAsync($"{BaseUrl}/friends");

            var friendListSection = Page.Locator("div:has(h2:text('Fronds'))");
            await Expect(friendListSection.GetByText("Harbor")).ToBeVisibleAsync();
        }

        [Test]
        public async Task ClickFriendsName_FriendsListPage_RedirectsToFriendProfile()
        {
            // NOTE: each test is set up as signed in with Finn and goes to /friends
            // Click friend's name
            await Page.GetByTestId("Shelly").ClickAsync();
            // redirects to Shelly's profile page
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/2");
            // Expect Shelly's friends' names to be visible
            await Expect(Page.GetByTestId("Finn")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("Bruce")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("Coral")).ToBeVisibleAsync();
        }

        // [Test]
        // public async Task ClickFriendsName_FriendsListPage_OpenFullFriendList()
        // {
        //     await Page.GetByTestId("Shelly").ClickAsync();
        //     await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/2");
        //     await Page.GetByTestId("see-all-friends").ClickAsync();

        //     await Expect(Page.GetByTestId("Bruce")).ToBeVisibleAsync();
        //     await Expect(Page.GetByTestId("Coral")).ToBeVisibleAsync(); await Expect(Page.GetByTestId("Bluey")).ToBeVisibleAsync();
        // }

        // [Test]
        // public async Task ClickFriendsName_FriendsListPage_AddANewFriend()
        // {
        //     await Page.GotoAsync("/users/4");
        //     await Page.GetByTestId("see-all-friends").ClickAsync();

        //     await Expect(Page.GetByTestId("Tigra")).ToBeVisibleAsync();
        //     await Page.GetByTestId("add-friend").ClickAsync();
        //     Page.Dialog += async (_, dialog) =>
        //         {
        //             Assert.That(dialog.Message, Does.Contain("Are you sure"));
        //             await dialog.AcceptAsync();
        //         };
        //     await Expect(Page.GetByRole(AriaRole.Button, new() {Name = "Pending Request"})).ToBeVisibleAsync();
        // }
    }
}