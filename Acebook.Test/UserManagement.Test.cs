using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models; 
using Acebook.Test;     

namespace Acebook.Tests
{
  public class UserManagementPlaywright : PageTest
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
    public async Task SignUp_ValidCredentials_RedirectToSignIn()
    {
      await Page.GotoAsync("/");

      await Page.Locator("#signup").ClickAsync();
      await Page.Locator("#firstname").FillAsync("Francine");
      await Page.Locator("#lastname").FillAsync("Gills");
      await Page.Locator("#dob").FillAsync("1995-08-10");
      await Page.Locator("#email").FillAsync("francine@sharkmail.ocean");
      await Page.Locator("#password").FillAsync("password123");
      await Page.Locator("#confirmpassword").FillAsync("password123");
      await Page.Locator("#submit").ClickAsync();

      await Expect(Page).ToHaveURLAsync($"{BaseUrl}/posts");
    }

    [Test]
    public async Task SignUp_InValidCredentials_Error()
    {
      await Page.GotoAsync("/");

      await Page.Locator("#signup").ClickAsync();
      await Page.Locator("#firstname").FillAsync("Francine");
      await Page.Locator("#lastname").FillAsync("Gills");
      await Page.Locator("#dob").FillAsync("1995-08-10");
      await Page.Locator("#email").FillAsync("francine@sharkmail.ocean");
      await Page.Locator("#password").FillAsync("password123");
      await Page.Locator("#confirmpassword").FillAsync("password12");
      await Page.Locator("#submit").ClickAsync();

      var error = Page.Locator("#error-message");
      await Expect(error).ToBeVisibleAsync();
      await Expect(error).ToHaveTextAsync("Passwords do not match.");
    }

    [Test]
    public async Task SignIn_ValidCredentials_RedirectToPosts()
    {
      await Page.GotoAsync("/signin");

      await Page.Locator("#email").FillAsync("finn.white@sharkmail.ocean");
      await Page.Locator("#password").FillAsync("password123");
      await Page.Locator("#submit").ClickAsync();

      await Expect(Page).ToHaveURLAsync($"{BaseUrl}/posts");
    }

    [Test]
    public async Task SignIn_InValidPassword_Error()
    {
      await Page.GotoAsync("/signin");

      await Page.Locator("#email").FillAsync("finn.white@sharkmail.ocean");
      await Page.Locator("#password").FillAsync("password12");
      await Page.Locator("#submit").ClickAsync();

      //var error = Page.Locator("#error-message");
      //await Expect(error).ToBeVisibleAsync();
      //await Expect(error).ToHaveTextAsync("Incorrect email or password.");
      await Expect(Page.GetByText("Incorrect email or password.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task SignIn_UnRegisteredEmail_Error()
    {
      await Page.GotoAsync("/signin");

      await Page.Locator("#email").FillAsync("finn.white@sharkmail.com");
      await Page.Locator("#password").FillAsync("password123");
      await Page.Locator("#submit").ClickAsync();

      // var error = Page.Locator("#error-message");
      // await Expect(error).ToBeVisibleAsync();
      // await Expect(error).ToHaveTextAsync("Incorrect email or password.");
      await Expect(Page.GetByText("Incorrect email or password.")).ToBeVisibleAsync();

    }
    [Test]
    public async Task SignIn_InvalidEmail_Error()
    {
      await Page.GotoAsync("/signin");

      await Page.Locator("#email").FillAsync("finn.whitesharkmail.com");
      await Page.Locator("#password").FillAsync("password123");
      await Page.Locator("#submit").ClickAsync();

      // var error = Page.Locator("#error-message");
      // await Expect(error).ToBeVisibleAsync();
      // await Expect(error).ToHaveTextAsync("Incorrect email or password.");
      await Expect(Page.GetByText("Enter a valid email address")).ToBeVisibleAsync();

    }
    [Test]
    public async Task SignIn_BlankEmail_Error()
    {
      await Page.GotoAsync("/signin");
      await Page.Locator("#password").FillAsync("password123");
      await Page.Locator("#submit").ClickAsync();

      // var error = Page.Locator("#error-message");
      // await Expect(error).ToBeVisibleAsync();
      // await Expect(error).ToHaveTextAsync("Incorrect email or password.");
      await Expect(Page.GetByText("Email is required")).ToBeVisibleAsync();

    }
    [Test]
    public async Task SignIn_BlankPassword_Error()
    {
      await Page.GotoAsync("/signin");
      await Page.Locator("#email").FillAsync("finn.white@sharkmail.com");
      await Page.Locator("#submit").ClickAsync();

      // var error = Page.Locator("#error-message");
      // await Expect(error).ToBeVisibleAsync();
      // await Expect(error).ToHaveTextAsync("Incorrect email or password.");
      await Expect(Page.GetByText("Password is required")).ToBeVisibleAsync();

    }
    
  }
}