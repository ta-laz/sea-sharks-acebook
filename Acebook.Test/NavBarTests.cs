namespace Acebook.Test;

using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NUnit.Framework;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

public class NavBarTests
{
    ChromeDriver driver;

    [SetUp]
    public void Setup()
    {
        driver = new ChromeDriver();
    }

    [TearDown]
    public void TearDown()
    {
        if (driver != null)
        {
            driver.Quit();
            driver.Dispose();
        }
    }

    [Test]
    public async Task SignOut_WhenUserSignIn_SignsOut()
    {
        driver.Navigate().GoToUrl("https://localhost:7196/");

        await Task.Delay(2000);

        driver.FindElement(By.CssSelector("input[name='email']")).SendKeys("finn.white@sharkmail.ocean");
        driver.FindElement(By.CssSelector("input[name='password']")).SendKeys("da2cb7f780b225403e5487ce7d40feaa0283f663ce05c7882df100110e8aae86");
        driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        await Task.Delay(2000);


        driver.FindElement(By.CssSelector("button#signout")).Click();

        var Cookies = driver.Manage().Cookies.AllCookies;

        Assert.That(Cookies, Is.Empty);
    }



}