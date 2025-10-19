using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models; 
using Acebook.Test;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Acebook.TestHelpers;

namespace Acebook.Tests
{
  public class UserManagementPlaywright : PageTest
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
    }

    public override BrowserNewContextOptions ContextOptions()
      => new BrowserNewContextOptions
      {
        BaseURL = BaseUrl
      };

    [Test]
    public async Task SignUp_ValidCredentials_RedirectToSignIn()
    {

      await Page.GotoAsync("/signup");
      await Expect(Page).ToHaveURLAsync($"{BaseUrl}/signup");
      await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
      await Page.Locator("#firstname").FillAsync("Francine");
      await Page.Locator("#lastname").FillAsync("Gills");
      await Page.Locator("#dob").FillAsync("1995-08-10");
      await Page.Locator("#email").FillAsync("francine@sharkmail.ocean");
      await Page.Locator("#password").FillAsync("Password123!");
      await Page.Locator("#confirmpassword").FillAsync("Password123!");

      await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts", new() { Timeout = 4000 }),
                Page.Locator("#submit").ClickAsync()
            );

    }
    
    [Test]
    public async Task SignUp_AllBlank_Error()
    {
      
      await Page.GotoAsync("/signup");

      await Page.Locator("#submit").ClickAsync();
      
      await Expect(Page.Locator("#firstname-error")).ToBeVisibleAsync();
      await Expect(Page.Locator("#lastname-error")).ToBeVisibleAsync();
      await Expect(Page.Locator("#dob-error")).ToBeVisibleAsync();
      await Expect(Page.Locator("#email-error")).ToBeVisibleAsync();
      await Expect(Page.Locator("#password-error")).ToBeVisibleAsync();
      await Expect(Page.Locator("#confirmpassword-error")).ToBeVisibleAsync();
    }

    [Test]
    public async Task SignUp_PasswordDoNotMatch_Error()
    {
      await Page.GotoAsync("/");

      await Page.Locator("#signup").ClickAsync();
      await Page.Locator("#firstname").FillAsync("Francine");
      await Page.Locator("#lastname").FillAsync("Gills");
      await Page.Locator("#dob").FillAsync("1995-08-10");
      await Page.Locator("#email").FillAsync("francine@sharkmail.ocean");
      await Page.Locator("#password").FillAsync("Password123!");
      await Page.Locator("#confirmpassword").FillAsync("Password123!!");
      await Page.Locator("#submit").ClickAsync();
      var confirmError = Page.Locator("span[data-valmsg-for='ConfirmPassword']");
      await Expect(confirmError).ToBeVisibleAsync();
      await Expect(confirmError).ToHaveTextAsync("Passwords do not match.");
    }

    [Test]
    public async Task SignUp_InvalidPassword_Error()
    {
      await Page.GotoAsync("/");

      await Page.Locator("#signup").ClickAsync();
      await Page.Locator("#firstname").FillAsync("Francine");
      await Page.Locator("#lastname").FillAsync("Gills");
      await Page.Locator("#dob").FillAsync("1995-08-10");
      await Page.Locator("#email").FillAsync("francine@sharkmail.ocean");
      await Page.Locator("#password").FillAsync("Password123");
      await Page.Locator("#confirmpassword").FillAsync("Password123");
      await Page.Locator("#submit").ClickAsync();
      //await Expect(Page.GetByTestId("error-message")).ToBeVisibleAsync();
      var confirmError = Page.Locator("span[data-valmsg-for='Password']");
      await Expect(confirmError).ToBeVisibleAsync();
      await Expect(confirmError).ToHaveTextAsync("Password must be ≥ 8 characters and include an uppercase letter and a special character.");
    }

    [Test]
    public async Task SignUp_InvalidEmail_Error()
    {
      await Page.GotoAsync("/");

      await Page.Locator("#signup").ClickAsync();
      await Page.Locator("#firstname").FillAsync("Francine");
      await Page.Locator("#lastname").FillAsync("Gills");
      await Page.Locator("#dob").FillAsync("1995-08-10");
      await Page.Locator("#email").FillAsync("francinesharkmail.ocean");
      await Page.Locator("#password").FillAsync("Password123!");
      await Page.Locator("#confirmpassword").FillAsync("Password123!");
      await Page.Locator("#submit").ClickAsync();
      //await Expect(Page.GetByTestId("error-message")).ToBeVisibleAsync();
      var confirmError = Page.Locator("span[data-valmsg-for='Email']");
      await Expect(confirmError).ToBeVisibleAsync();
      await Expect(confirmError).ToHaveTextAsync("Enter a valid email address");
    }

    [Test]
    public async Task SignIn_ValidCredentials_RedirectToPosts()
    {
      await Page.GotoAsync("/signin");

      await Page.Locator("#email").FillAsync("finn.white@sharkmail.ocean");
      await Page.Locator("#password").FillAsync("password123");
      await Page.Locator("#signin-submit").ClickAsync();

      await Expect(Page).ToHaveURLAsync($"{BaseUrl}/posts", new() { Timeout = 4000 });
    }

    [Test]
    public async Task SignIn_InValidPassword_Error()
    {
      await Page.GotoAsync("/signin");

      await Page.Locator("#email").FillAsync("finn.white@sharkmail.ocean");
      await Page.Locator("#password").FillAsync("password12");
      await Page.Locator("#signin-submit").ClickAsync();
      //await Expect(Page.GetByTestId("error")).ToHaveTextAsync("Incorrect email or password.");
      var confirmError = Page.Locator(".validation-summary-errors");
      await Expect(confirmError).ToHaveTextAsync("Incorrect email or password.");
    }

    [Test]
    public async Task SignIn_UnRegisteredEmail_Error()
    {
      
      await Page.GotoAsync("/signin");
      await Page.Locator("#email").FillAsync("finn.white@sharkmail.com");
      await Page.Locator("#password").FillAsync("password123");
      await Page.Locator("#signin-submit").ClickAsync();
      //await Expect(Page.GetByTestId("error")).ToHaveTextAsync("Incorrect email or password.");
      var confirmError = Page.Locator(".validation-summary-errors");
      await Expect(confirmError).ToHaveTextAsync("Incorrect email or password.");
    }
    [Test]
    public async Task SignIn_InvalidEmail_Error()
    {
      await Page.GotoAsync("/signin");

      await Page.Locator("#email").FillAsync("finn.whitesharkmail.com");
      await Page.Locator("#password").FillAsync("password123");
      await Page.Locator("#signin-submit").ClickAsync();
      //var error = Page.Locator("Enter a valid email address");
      //await Expect(error).ToHaveTextAsync("Enter a valid email address");
      // await Expect(error).ToHaveTextAsync("Enter a valid email address");
      await Expect(Page.GetByText("Enter a valid email address")).ToBeVisibleAsync();

    }
    [Test]
    public async Task SignIn_BlankEmail_Error()
    {
      await Page.GotoAsync("/signin");
      await Page.Locator("#password").FillAsync("password123");
      await Page.Locator("#signin-submit").ClickAsync();

      //var error = Page.Locator("#Email-error");
      //await Expect(error).ToBeVisibleAsync();
      // await Expect(error).ToHaveTextAsync("Incorrect email or password.");
      await Expect(Page.GetByText("Email is required")).ToBeVisibleAsync();

    }
    [Test]
    public async Task SignIn_BlankPassword_Error()
    {
      await Page.GotoAsync("/signin");
      await Page.Locator("#email").FillAsync("finn.white@sharkmail.com");
      await Page.Locator("#signin-submit").ClickAsync();

      //var error = Page.Locator("#Password-error");
      //await Expect(error).ToBeVisibleAsync();
      // await Expect(error).ToHaveTextAsync("Incorrect email or password.");
      await Expect(Page.GetByText("Password is required")).ToBeVisibleAsync();

    }
    
  }
}