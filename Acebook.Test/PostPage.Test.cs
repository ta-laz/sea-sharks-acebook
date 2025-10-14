using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;
using System.Text.RegularExpressions;


namespace Acebook.Tests
{
    public class PostPagePlaywright : PageTest
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
            // Accept all confirmation popups
            Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

            // Go to sign-in page
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
        public async Task CommentButton_ClickedOnPost_NavigatesToPostPage()
        {
            
            // Click on a post:
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts/175"),
                Page.GetByTestId("comment-button").First.ClickAsync()
            );
            await Expect(Page.Locator("#splash-heading")).ToContainTextAsync("Bluey's Splash");
        }


        [Test]
        public async Task SeeMore_ClickedOnPost_NavigatesToPostPage()
        {
            // Click on a post:
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts/175"),
                Page.GetByTestId("see-more-button").First.ClickAsync()
            );
            await Expect(Page.Locator("#splash-heading")).ToContainTextAsync("Bluey's Splash");
        }


        [Test]
        public async Task SubmitButton_ClickedOnPost_AddsCommentToPost()
        {
            // Click on a post:
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts/175"),
                Page.GetByTestId("see-more-button").First.ClickAsync()
            );

            // Checks you are on that post page:
            await Expect(Page.Locator("#splash-heading")).ToContainTextAsync("Bluey's Splash");

            // Click into the comment box
            await Page.Locator("#comment-box").ClickAsync();

            // Input comment
            await Page.Locator("#comment-box").FillAsync("Test Comment");

            // Click submit
            await Page.GetByTestId("comment-submit").ClickAsync();

            // Comment appears on page
            await Expect(Page.GetByText("Test Comment")).ToBeVisibleAsync();
        }


        // NOTE: This test is a bit flimsy (if we add a comment to post 175 it will no longer test for the No Comments message (because it won't be displayed) but I liked the if statement??)
        [Test]
        public async Task PostPage_NoComments_DisplaysNoCommentsMessage()
        {
            // Click on a post:
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts/175"),
                Page.GetByTestId("see-more-button").First.ClickAsync()
            );

            // Checks you are on that post page:
            await Expect(Page.Locator("#splash-heading")).ToContainTextAsync("Bluey's Splash");

            // Check for comments section
            var comments = Page.Locator(".comment-item"); // use a selector for your comment divs
            var commentCount = await comments.CountAsync();

            if (commentCount == 0)
            {
                // Expect "No comments yet!" message if there are no comments
                await Expect(Page.GetByText("No comments yet!")).ToBeVisibleAsync();
            }
            else
            {
                // Otherwise expect at least one comment visible
                await Expect(comments.First).ToBeVisibleAsync();
            }
        }


        [Test]
        public async Task NewPostWithNoComments_DisplaysNoCommentsMessage()
        {
            // Create post
            await Page.Locator("#post-content").FillAsync("Test content");
            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/posts")
            );

            // Wait for the new post to appear on the posts page
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();

            // Click “See more” on the new post
            await Task.WhenAll(
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+")), // Uses regex \\d+ to expect one or more digits, scalable when more posts are added 
                Page.GetByTestId("see-more-button").First.ClickAsync() // Because we have just made the post, we can still click just on the first one 
            );

            // Confirm we are on an individual post page
            await Expect(Page.Locator("#splash-heading")).ToBeVisibleAsync();

            // Confirm we are on the NEW individual post page
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();

            // Check for “No comments yet!” message
            await Expect(Page.GetByText("No comments yet!")).ToBeVisibleAsync();
        }

        [Test]
        
        public async Task LikeAPostDynamicallyUpdatesLikeTotal()
        {
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts/175"),
                Page.GetByTestId("see-more-button").First.ClickAsync()
            );
            await Expect(Page.Locator("#splash-heading")).ToContainTextAsync("Bluey's Splash");
            await Page.GetByTestId("post-like-button").ClickAsync();
            await Expect(Page.GetByTestId("post-like-total")).ToContainTextAsync("Like (3)");
        }
        [Test]
        
        public async Task LikeButtonDynamicallyUpdatesOnComments()
        {
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts/175"),
                Page.GetByTestId("see-more-button").First.ClickAsync()
            );
            await Expect(Page.Locator("#splash-heading")).ToContainTextAsync("Bluey's Splash");
            await Page.Locator("#comment-box").ClickAsync();
            await Page.Locator("#comment-box").FillAsync("Test Comment");
            await Page.GetByTestId("comment-submit").ClickAsync();
            await Page.GetByTestId("comment-like-button").ClickAsync();
            await Expect(Page.GetByTestId("comment-like-total")).ToContainTextAsync("Like (1)");
        }




        /// TESTS FOR DELETING COMMENTS AND POSTS:

        [Test]
        public async Task DeletePost_PostAuthor_DeletesPost()
        {
            // Create post
            await Page.Locator("#post-content").FillAsync("Test delete post button works properly");
            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/posts")
            );

            // Wait for the new post to appear on the posts page
            await Expect(Page.GetByText("Test delete post button works properly")).ToBeVisibleAsync();

            // Click “See more” on the new post
            await Task.WhenAll(
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+")), // Uses regex \\d+ to expect one or more digits, scalable when more posts are added 
                Page.GetByTestId("see-more-button").First.ClickAsync() // Because we have just made the post, we can still click just on the first one 
            );

            // Confirm we are on an individual post page
            await Expect(Page.Locator("#splash-heading")).ToBeVisibleAsync();

            // Confirm we are on the NEW individual post page
            await Expect(Page.GetByText("Test delete post button works properly")).ToBeVisibleAsync();

            // Click 'Delete Post' button:
            await Task.WhenAll(
                Page.GetByTestId("delete-post-button").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/posts")
            );

            // Confirm the post is no longer visible on Aquarium:
            await Expect(Page.GetByText("Test delete post button works properly")).Not.ToBeVisibleAsync();
        }


        [Test]
        public async Task DeletePost_NotPostAuthor_CantDeletePost()
        {
            // Click “See more” on the top post (not ours hopefully)
            await Task.WhenAll(
                Page.GetByTestId("see-more-button").First.ClickAsync()
            );
            // Confirm we are on an individual post page
            await Expect(Page.Locator("#splash-heading")).ToBeVisibleAsync();

            // Confirm the delete comment button is not visible
            await Expect(Page.GetByTestId("delete-post-button")).Not.ToBeVisibleAsync();
        }


        [Test]
        public async Task DeleteComment_CommentAuthor_CanDeleteComment()
        {
            // Click “See more” on the top post (not ours hopefully)
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            // Confirm we are on an individual post page
            await Expect(Page.Locator("#splash-heading")).ToBeVisibleAsync();

            // Create comment
            await Page.Locator("#comment-box").FillAsync("Test comment");

            await Page.ScreenshotAsync(new() { Path = "before_submitted_comment.png" });
            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.GetByTestId("comment-submit").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            await Page.ScreenshotAsync(new() { Path = "after_submitted_comment.png" });

            await Expect(Page.GetByText("Test comment")).ToBeVisibleAsync();

            // Click 'Delete Comment' button:
            await Task.WhenAll(
                Page.GetByTestId("delete-comment-button").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            // Confirm the post is no longer visible on Aquarium:
            await Expect(Page.GetByText("Test comment")).Not.ToBeVisibleAsync();
        }

        

        [Test]
        public async Task DeleteComment_NOTCommentAuthor_CannotDeleteComment()
        {
            // Click “See more” on the top post (not ours hopefully)
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            // Create comment
            await Page.Locator("#comment-box").FillAsync("Test comment");

            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.GetByTestId("comment-submit").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            await Expect(Page.GetByText("Test comment")).ToBeVisibleAsync();

            // Signout:
            await Task.WhenAll(
                Page.Locator("#dropdownDefaultButton").ClickAsync(),
                Page.GetByTestId("signout").ClickAsync()
            );

            // Wait for form to load
            await Page.WaitForSelectorAsync("#signin-submit", new() { State = WaitForSelectorState.Visible });
            
            // Sign in with different details 
            await Page.Locator("#email").FillAsync("shelly.tiger@sharkmail.ocean");
            await Page.Locator("#password").FillAsync("password123");
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts"),
                Page.Locator("#signin-submit").ClickAsync()
            );

            await Page.ScreenshotAsync(new() { Path = "after_signed_in.png" });

            // Click “See more” on the top post (not ours hopefully)
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            await Page.ScreenshotAsync(new() { Path = "after_clicked_new_post.png" });


            // Confirm the delete comment button is not visible
            await Expect(Page.GetByTestId("delete-comment-button")).Not.ToBeVisibleAsync();
        }
    }
}