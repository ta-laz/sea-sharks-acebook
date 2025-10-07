using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Acebook.Test;
using acebook.Models;

namespace Acebook.Tests
{
  public class UserManagement
  {
    ChromeDriver driver;


    [OneTimeSetUp]
    public async Task OneTime()
    {
        await using var context = new AcebookDbContext();
        await TestDataSeeder.EnsureDbReadyAsync(context);
    }

    [SetUp]
    public async Task Setup()
    {
      driver = new ChromeDriver();
        await using var context = new AcebookDbContext();
        await TestDataSeeder.ResetAndSeedAsync(context);
    }

    [TearDown]
    public void TearDown() {
      driver.Quit();
      driver.Dispose();
    }

    [Test]
    public void SignUp_ValidCredentials_RedirectToSignIn()
    {
      driver.Navigate().GoToUrl("http://127.0.0.1:5287");
      IWebElement signUpButton = driver.FindElement(By.Id("signup"));
      signUpButton.Click();
      IWebElement nameField = driver.FindElement(By.Id("name"));
      nameField.SendKeys("francine");
      IWebElement emailField = driver.FindElement(By.Id("email"));
      emailField.SendKeys("francine@email.com");
      IWebElement passwordField = driver.FindElement(By.Id("password"));
      passwordField.SendKeys("12345678");
      IWebElement submitButton = driver.FindElement(By.Id("submit"));
      submitButton.Click();
      string currentUrl = driver.Url;
      Assert.That(currentUrl, Is.EqualTo("http://127.0.0.1:5287/signin"));
    }

    [Test]
    public void SignIn_ValidCredentials_RedirectToPosts() {

      driver.Navigate().GoToUrl("http://127.0.0.1:5287/signin");
      IWebElement emailField = driver.FindElement(By.Id("email"));
      IWebElement passwordField = driver.FindElement(By.Id("password"));
      IWebElement submitButton = driver.FindElement(By.Id("submit"));
      // emailField = driver.FindElement(By.Id("email"));
      emailField.SendKeys("finn.white@sharkmail.ocean");
      // passwordField = driver.FindElement(By.Id("password"));
      passwordField.SendKeys("da2cb7f780b225403e5487ce7d40feaa0283f663ce05c7882df100110e8aae86");
      // submitButton = driver.FindElement(By.Id("submit"));
      submitButton.Click();
      string currentUrl = driver.Url;
      Assert.That(currentUrl, Is.EqualTo("http://127.0.0.1:5287/posts"));
    }
  }
}