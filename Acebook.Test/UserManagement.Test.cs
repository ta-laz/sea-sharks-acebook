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
    public void TearDown()
    {
      driver.Quit();
      driver.Dispose();
    }

    [Test]
    public void SignUp_ValidCredentials_RedirectToSignIn()
    {
      driver.Navigate().GoToUrl("http://127.0.0.1:5287");
      IWebElement signUpButton = driver.FindElement(By.Id("signup"));
      signUpButton.Click();
      IWebElement firstNameField = driver.FindElement(By.Id("firstname"));
      firstNameField.SendKeys("Francine");
      IWebElement lastNameField = driver.FindElement(By.Id("lastname"));
      lastNameField.SendKeys("Gills");
      IWebElement emailField = driver.FindElement(By.Id("email"));
      emailField.SendKeys("francine@sharkmail.ocean");
      IWebElement passwordField = driver.FindElement(By.Id("password"));
      passwordField.SendKeys("password123");
      IWebElement confirmPasswordField = driver.FindElement(By.Id("confirmpassword"));
      confirmPasswordField.SendKeys("password123");
      IWebElement submitButton = driver.FindElement(By.Id("submit"));
      submitButton.Click();
      string currentUrl = driver.Url;
      Assert.That(currentUrl, Is.EqualTo("http://127.0.0.1:5287/signin"));
    }

    [Test]
    public void SignUp_InValidCredentials_Error()
    {
      driver.Navigate().GoToUrl("http://127.0.0.1:5287");
      IWebElement signUpButton = driver.FindElement(By.Id("signup"));
      signUpButton.Click();
      IWebElement firstNameField = driver.FindElement(By.Id("firstname"));
      firstNameField.SendKeys("Francine");
      IWebElement lastNameField = driver.FindElement(By.Id("lastname"));
      lastNameField.SendKeys("Gills");
      IWebElement emailField = driver.FindElement(By.Id("email"));
      emailField.SendKeys("francine@sharkmail.ocean");
      IWebElement passwordField = driver.FindElement(By.Id("password"));
      passwordField.SendKeys("password123");
      IWebElement confirmPasswordField = driver.FindElement(By.Id("confirmpassword"));
      confirmPasswordField.SendKeys("password12");
      IWebElement submitButton = driver.FindElement(By.Id("submit"));
      submitButton.Click();
      IWebElement error = driver.FindElement(By.Id("error-message"));
      Assert.That(error.Text, Is.EqualTo("Passwords do not match."));
    }

    [Test]
    public void SignIn_ValidCredentials_RedirectToPosts()
    {

      driver.Navigate().GoToUrl("http://127.0.0.1:5287/signin");
      IWebElement emailField = driver.FindElement(By.Id("email"));
      IWebElement passwordField = driver.FindElement(By.Id("password"));
      IWebElement submitButton = driver.FindElement(By.Id("submit"));
      emailField.SendKeys("finn.white@sharkmail.ocean");
      passwordField.SendKeys("password123");
      submitButton.Click();
      string currentUrl = driver.Url;
      Assert.That(currentUrl, Is.EqualTo("http://127.0.0.1:5287/posts"));
    }

    [Test]
    public void SignIn_InValidPassword_Error()
    {

      driver.Navigate().GoToUrl("http://127.0.0.1:5287/signin");
      IWebElement emailField = driver.FindElement(By.Id("email"));
      IWebElement passwordField = driver.FindElement(By.Id("password"));
      IWebElement submitButton = driver.FindElement(By.Id("submit"));
      emailField.SendKeys("finn.white@sharkmail.ocean");
      passwordField.SendKeys("password12");
      submitButton.Click();
      IWebElement error = driver.FindElement(By.Id("error-message"));
      Assert.That(error.Text, Is.EqualTo("Incorrect email or password."));
    }
    
    [Test]
    public void SignIn_InValidEmail_Error() {

      driver.Navigate().GoToUrl("http://127.0.0.1:5287/signin");
      IWebElement emailField = driver.FindElement(By.Id("email"));
      IWebElement passwordField = driver.FindElement(By.Id("password"));
      IWebElement submitButton = driver.FindElement(By.Id("submit"));
      emailField.SendKeys("finn.white@sharkmail.com");
      passwordField.SendKeys("password123");
      submitButton.Click();
      IWebElement error = driver.FindElement(By.Id("error-message"));
      Assert.That(error.Text, Is.EqualTo("Incorrect email or password."));
    }
  }
}