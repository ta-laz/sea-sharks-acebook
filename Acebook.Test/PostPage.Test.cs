using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;
using System.Text.RegularExpressions;
using Acebook.TestHelpers;


namespace Acebook.Tests
{
    public class PostPagePlaywright : PageTest
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
            // Accept all confirmation popups
            Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

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

            // Look for the new comment (using id to avoid the hidden form):
            await Expect(Page.Locator("p[id^='comment_content-']", new() { HasTextString = "Test comment" })).ToBeVisibleAsync();
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
            await Page.GetByTestId("post-content-input").FillAsync("Test content");
            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/posts")
            );

            // Wait for the new post to appear on the posts page
            await Expect(Page.GetByText("Test content")).ToBeVisibleAsync();

            // Click “Sea More” on the new post
            await Task.WhenAll(
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+")), // Uses regex \\d+ to expect one or more digits, scalable when more posts are added 
                Page.GetByTestId("see-more-button").First.ClickAsync() // Because we have just made the post, we can still click just on the first one 
            );

            // Confirm we are on an individual post page
            await Expect(Page.Locator("#splash-heading")).ToBeVisibleAsync();

            // Check for “No comments yet!” message
            await Expect(Page.GetByText("No comments yet!")).ToBeVisibleAsync();
        }


        /// TESTS FOR LIKE TOTALS

        [Test]

        public async Task LikeAPostDynamicallyUpdatesLikeTotal()
        {
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts/175"),
                Page.GetByTestId("see-more-button").First.ClickAsync()
            );
            await Expect(Page.Locator("#splash-heading")).ToContainTextAsync("Bluey's Splash");
            await Page.GetByTestId("post-like-button").ClickAsync();
            await Expect(Page.GetByTestId("post-like-total")).ToContainTextAsync("Like (2)");
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
            await Page.GetByTestId("post-content-input").FillAsync("Test delete post button works properly");
            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/posts")
            );

            // Wait for the new post to appear on the posts page
            await Expect(Page.GetByText("Test delete post button works properly")).ToBeVisibleAsync();

            // Click “Sea More” on the new post
            await Task.WhenAll(
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+")), // Uses regex \\d+ to expect one or more digits, scalable when more posts are added 
                Page.GetByTestId("see-more-button").First.ClickAsync() // Because we have just made the post, we can still click just on the first one 
            );

            // Confirm we are on an individual post page
            await Expect(Page.Locator("#splash-heading")).ToBeVisibleAsync();

            // Confirm we are on the NEW individual post page
            await Expect(Page.GetByTestId("post-content")).ToContainTextAsync("Test delete post button works properly");

            // Click 'Delete Post' button:
            await Task.WhenAll(
                Page.GetByTestId("delete-post-button").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/users/1")
            );

            // Confirm the post is no longer visible on Own Profile:
            await Expect(Page.GetByText("Test delete post button works properly")).Not.ToBeVisibleAsync();
        }


        [Test]
        public async Task DeletePost_NotPostAuthor_CannotDeletePost()
        {
            // Create post
            await Page.GetByTestId("post-content-input").FillAsync("Test delete post button works properly");
            
            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/posts")
            );

            // Wait for the new post to appear on the posts page
            await Expect(Page.GetByText("Test delete post button works properly")).ToBeVisibleAsync();

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

            // Click “Sea More” on the top post (just made)
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            // Confirm we are on an individual post page
            await Expect(Page.Locator("#splash-heading")).ToBeVisibleAsync();

            // Confirm the delete comment button is not visible
            await Expect(Page.GetByTestId("delete-post-button")).Not.ToBeVisibleAsync();  
        

        }


        [Test]
        public async Task DeleteComment_CommentAuthor_CanDeleteComment()
        {
            // Click “Sea More” on the top post
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            // Confirm we are on an individual post page
            await Expect(Page.Locator("#splash-heading")).ToBeVisibleAsync();

            // Create comment
            await Page.Locator("#comment-box").FillAsync("Test comment");

            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.GetByTestId("comment-submit").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            // Find new comment (using id to avoid the hidden form), save as variable:
            var newComment = Page.Locator("p[id^='comment_content-']", new() { HasTextString = "Test comment" });

            // Expect the new comment to be visible on the page:
            await Expect(newComment).ToBeVisibleAsync();

            // Extract the id attribute to use later:
            var commentId = await newComment.GetAttributeAsync("id");

            // Isolate the id number from the commentId (commentId will be like "comment_content-351")
            commentId = commentId.Split('-').Last();  // gives "351"

            // Click 'Delete Comment' button:
            await Task.WhenAll(
                Page.GetByTestId($"delete-comment-button-{commentId}").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            await Page.ScreenshotAsync(new() { Path = "after_deleted_comment.png" });

            // Confirm the comment is no longer visible on Post:
            await Expect(Page.Locator($"comment-text-{commentId}")).Not.ToBeVisibleAsync();

        }

        [Test]
        public async Task DeleteComment_CommentAuthor_CanDeleteRIGHTComment()
        {
            // Click “Sea More” on the top post
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            // Confirm we are on an individual post page
            await Expect(Page.Locator("#splash-heading")).ToBeVisibleAsync();

            // Create comment
            await Page.Locator("#comment-box").FillAsync("Test comment");

            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.GetByTestId("comment-submit").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            // Find new comment (using id to avoid the hidden form), save as variable:
            var newComment = Page.Locator("p[id^='comment_content-']", new() { HasTextString = "Test comment" });

            // Expect the new comment to be visible on the page:
            await Expect(newComment).ToBeVisibleAsync();

            // Extract the id attribute to use later:
            var commentId = await newComment.GetAttributeAsync("id");

            // Isolate the id number from the commentId (commentId will be like "comment_content-351")
            commentId = commentId.Split('-').Last();  // gives "351"

            // Create another comment
            await Page.Locator("#comment-box").FillAsync("Test comment2");

            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.GetByTestId("comment-submit").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            // Click 'Delete Comment' button on FIRST comment not second comment:
            await Task.WhenAll(
                Page.GetByTestId($"delete-comment-button-{commentId}").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            // Confirm the FIRST comment is no longer visible on Post:
            await Expect(Page.Locator($"comment-text-{commentId}")).Not.ToBeVisibleAsync();

        }



        [Test]
        public async Task DeleteComment_NOTCommentAuthor_CannotDeleteComment()
        {
            // Click “Sea More” on the top post
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            // Create comment
            await Page.Locator("#comment-box").FillAsync("Test comment");

            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.GetByTestId("comment-submit").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            // Find new comment (using id to avoid the hidden form), save as variable:
            var newComment = Page.Locator("p[id^='comment_content-']", new() { HasTextString = "Test comment" });

            // Expect the new comment to be visible on the page:
            await Expect(newComment).ToBeVisibleAsync();

            // Extract the id attribute to use later:
            var commentId = await newComment.GetAttributeAsync("id");

            // Isolate the id number from the commentId (commentId will be like "comment_content-351")
            commentId = commentId.Split('-').Last();  // gives "351"

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

            // Click “Sea More” on the top post (just made)
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            // Confirm the delete comment button is not visible
            await Expect(Page.GetByTestId($"delete-comment-button-{commentId}")).Not.ToBeVisibleAsync();
        }




        /// TESTS FOR UPDATING COMMENTS AND POSTS:

        [Test]
        public async Task UpdatePost_PostAuthor_CanUpdatePost()
        {
            // Create post
            await Page.GetByTestId("post-content-input").FillAsync("Test Edit post button works properly");

            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/posts")
            );

            // Wait for the new post to appear on the posts page
            await Expect(Page.GetByText("Test Edit post button works properly")).ToBeVisibleAsync();

            // Click “Sea More” on the new post
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            // Expect the Edit Post Button to be visible on the page:
            await Expect(Page.GetByTestId("edit-post-button")).ToBeVisibleAsync();

            // Click on Edit Post Button
            await Page.GetByTestId("edit-post-button").ClickAsync();

            // Edit post
            await Page.GetByTestId("update-post-input").FillAsync("Test Edit post button works properly - edited");

            // Click on Save Changes Button
            await Task.WhenAll(
                Page.GetByTestId("edit-post-submit").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            // Confirm the Edited post is visible
            await Expect(Page.GetByTestId("post-content")).ToHaveTextAsync("Test Edit post button works properly - edited");
        }
        
        [Test]
        public async Task UpdatePost_NOTPostAuthor_CannotUpdatePost()
        {
            // Create post
            await Page.GetByTestId("post-content-input").FillAsync("Test Edit post button works properly");

            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.Locator("#post-submit").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/posts")
            );

            // Wait for the new post to appear on the posts page
            await Expect(Page.GetByText("Test Edit post button works properly")).ToBeVisibleAsync();

            // Click “Sea More” on the first post
            await Page.GetByTestId("see-more-button").First.ClickAsync();

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

            // Click “Sea More” on the top post (just made)
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            // Confirm the update post button is not visible
            await Expect(Page.GetByTestId("edit-post-button")).Not.ToBeVisibleAsync();
        }

        [Test]
        public async Task UpdateComment_CommentAuthor_CanUpdateComment()
        {
            // Click “Sea More” on the first post
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            // Create comment
            await Page.Locator("#comment-box").FillAsync("Test comment");

            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.GetByTestId("comment-submit").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            // Find new comment (using id to avoid the hidden form), save as variable:
            var newComment = Page.Locator("p[id^='comment_content-']", new() { HasTextString = "Test comment" });

            // Expect the new comment to be visible on the page:
            await Expect(newComment).ToBeVisibleAsync();

            // Extract the id attribute to use later:
            var commentId = await newComment.GetAttributeAsync("id");

            // Isolate the id number from the commentId (commentId will be like "comment_content-351")
            commentId = commentId.Split('-').Last();  // gives "351"


            // Confirm the Edit comment button is visible
            await Expect(Page.GetByTestId($"edit-comment-button-{commentId}")).ToBeVisibleAsync();

            // Click on Edit Comment Button
            await Page.GetByTestId($"edit-comment-button-{commentId}").ClickAsync();

            // Edit comment
            await Page.GetByTestId($"update-comment-input-{commentId}").FillAsync("Test comment - edited");

            await Page.ScreenshotAsync(new() { Path = "added_edit_to_comment.png" });

            // Click on Save Changes Button
            await Task.WhenAll(
                Page.GetByTestId($"edit-comment-submit-{commentId}").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            // Confirm the Edited comment is visible
            await Expect(Page.GetByTestId($"comment-text-{commentId}")).ToHaveTextAsync("Test comment - edited");
        }


        [Test]
        public async Task UpdateComment_NOTCommentAuthor_CannotUpdateComment()
        {
            // Click “Sea More” on the first post
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            // Create comment
            await Page.Locator("#comment-box").FillAsync("Test comment");

            // Wait for post submission + redirect
            await Task.WhenAll(
                Page.GetByTestId("comment-submit").ClickAsync(),
                Page.WaitForURLAsync(new Regex($"{BaseUrl}/posts/\\d+"))
            );

            // Find new comment (using id to avoid the hidden form), save as variable:
            var newComment = Page.Locator("p[id^='comment_content-']", new() { HasTextString = "Test comment" });

            // Expect the new comment to be visible on the page:
            await Expect(newComment).ToBeVisibleAsync();

            // Extract the id attribute to use later:
            var commentId = await newComment.GetAttributeAsync("id");

            // Isolate the id number from the commentId (commentId will be like "comment_content-351")
            commentId = commentId.Split('-').Last();  // gives "351"

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

            // Click “Sea More” on the top post (just made)
            await Page.GetByTestId("see-more-button").First.ClickAsync();

            // Confirm the update comment button is not visible
            await Expect(Page.GetByTestId($"update-comment-button-{commentId}")).Not.ToBeVisibleAsync();
        }
    }
}


///SCREENSHOT SYNTAX:
/// 
/// await Page.ScreenshotAsync(new() { Path = "after_clicked_new_post.png" });