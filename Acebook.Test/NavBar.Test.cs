using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;
using System.Data.Common;
using Acebook.TestHelpers;

namespace Acebook.Tests
{
    public class NavBarPlaywright : PageTest
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
        public async Task SignOut_WhenUserSignedIn_SignsOut()
        {

            await Page.GotoAsync("/signin");

            await Page.Locator("#email").FillAsync("finn.white@sharkmail.ocean");
            await Page.Locator("#password").FillAsync("password123");
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts", new() { Timeout = 4000 }), 
                Page.GetByTestId("signin-submit").ClickAsync()
            );
            await Page.ClickAsync("#dropdownDefaultButton");
            await Expect(Page.Locator("#signout")).ToBeVisibleAsync(); // key wait
            await Page.ClickAsync("#signout");
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/signin");
        }

        [Test]
        public async Task MyProfileButton_SignedInUser_RedirectsToUserIndexPage()
        {
            await Page.GotoAsync("/signin");
            await Page.Locator("#email").FillAsync("finn.white@sharkmail.ocean");
            await Page.Locator("#password").FillAsync("password123");
            await Task.WhenAll(
                Page.WaitForURLAsync($"{BaseUrl}/posts", new() { Timeout = 4000 }), 
                Page.GetByTestId("signin-submit").ClickAsync()
            );
            await Page.ClickAsync("#dropdownDefaultButton");
            await Page.ClickAsync("#MyProfile");
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users/1");
        }
    }
}

// namespace Acebook.Test;

// using OpenQA.Selenium.Chrome;
// using OpenQA.Selenium;
// using Microsoft.AspNetCore.Mvc.RazorPages;
// using NUnit.Framework;
// using System.Data.Common;
// using System.Diagnostics.CodeAnalysis;

// public class NavBarTests
// {
//     ChromeDriver driver;

//     [SetUp]
//     public void Setup()
//     {
//         driver = new ChromeDriver();
//     }

//     [TearDown]
//     public void TearDown()
//     {
//         if (driver != null)
//         {
//             driver.Quit();
//             driver.Dispose();
//         }
//     }

//     [Test]
//     public async Task SignOut_WhenUserSignIn_SignsOut()
//     {
//         driver.Navigate().GoToUrl("https://localhost:7196/");

//         await Task.Delay(2000);

//         driver.FindElement(By.CssSelector("input[name='email']")).SendKeys("finn.white@sharkmail.ocean");
//         driver.FindElement(By.CssSelector("input[name='password']")).SendKeys("da2cb7f780b225403e5487ce7d40feaa0283f663ce05c7882df100110e8aae86");
//         driver.FindElement(By.CssSelector("button[type='submit']")).Click();

//         await Task.Delay(2000);


//         driver.FindElement(By.CssSelector("button#signout")).Click();

//         var Cookies = driver.Manage().Cookies.AllCookies;

//         Assert.That(Cookies, Is.Empty);
//     }



// }