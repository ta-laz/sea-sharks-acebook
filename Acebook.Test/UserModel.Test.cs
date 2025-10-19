using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using acebook.Models;
using Acebook.Test;
using Acebook.TestHelpers;

namespace Acebook.Tests
{
    public class UserModelTestsPlaywright : PageTest
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
        public void FormattedCreatedOnReturnsSuffixSt()
        {
            Post post = new() { UserId = 1, WallId = 1, Content = "Just circled the reef with @Shelly Tiger — great current today!", CreatedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
            Assert.That(post.FormattedCreatedOn, Is.EqualTo("Wednesday 1st January 2025, 12:00 AM"));
        }

        [Test]
        public void FormattedCreatedOnReturnsSuffixNd()
        {
            Post post = new() { UserId = 1, WallId = 1, Content = "Just circled the reef with @Shelly Tiger — great current today!", CreatedOn = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc) };
            Assert.That(post.FormattedCreatedOn, Is.EqualTo("Thursday 2nd January 2025, 12:00 AM"));
        }

        [Test]
        public void FormattedCreatedOnReturnsSuffixRd()
        {
            Post post = new() { UserId = 1, WallId = 1, Content = "Just circled the reef with @Shelly Tiger — great current today!", CreatedOn = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc) };
            Assert.That(post.FormattedCreatedOn, Is.EqualTo("Friday 3rd January 2025, 12:00 AM"));
        }

        [Test]
        public void FormattedCreatedOnReturnsSuffixTh()
        {
            Post post = new() { UserId = 1, WallId = 1, Content = "Just circled the reef with @Shelly Tiger — great current today!", CreatedOn = new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc) };
            Assert.That(post.FormattedCreatedOn, Is.EqualTo("Saturday 4th January 2025, 12:00 AM"));
        }

        [Test]
        public void CheckLengthReturnsTrueForShortContentString()
        {
            Post post = new() { UserId = 1, WallId = 1, Content = "Test!", CreatedOn = new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc) };
            Assert.That(post.CheckLength(), Is.EqualTo(true));
        }

        [Test]
        public void CheckLengthReturnsFalseForLongContentString()
        {
            Post post = new() { UserId = 1, WallId = 1, Content = "Just circled the reef with @Shelly Tiger — great current today! Just circled the reef with @Shelly Tiger — great current today! Just circled the reef with @Shelly Tiger — great current today! Just circled the reef with @Shelly Tiger — great current today! Just circled the reef with @Shelly Tiger — great current today! Just circled the reef with @Shelly Tiger — great current today! Just circled the reef with @Shelly Tiger — great current today! Just circled the reef with @Shelly Tiger — great current today! Just circled the reef with @Shelly Tiger — great current today! Just circled the reef with @Shelly Tiger — great current today!", CreatedOn = new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc) };
            Assert.That(post.CheckLength(), Is.EqualTo(false));
        }

        [Test]
        public void FormatPostContentReturnsStringWithEllipsisIfOver500Chars()
        {
            Post post = new() { UserId = 1, WallId = 1, Content = "12345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891test!", CreatedOn = new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc) };
            Assert.That(post.FormatPostContent(), Is.EqualTo("12345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891123456789112345678911234567891..."));
        }
    }
}