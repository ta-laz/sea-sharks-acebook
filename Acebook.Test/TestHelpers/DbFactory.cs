//Acebook.Test/TestHelpers/DbFactory.cs
using Microsoft.EntityFrameworkCore;
using acebook.Models;
using System.IO;

namespace Acebook.TestHelpers
{
    public static class DbFactory
    {
        public static AcebookDbContext CreateTestDb()
        {
            try
            {
                // Load .env so tests share the same DB as the app by default
                if (File.Exists(".env")) DotNetEnv.Env.Load(".env");
            }
            catch {}

            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            var name = Environment.GetEnvironmentVariable("DB_NAME") ?? "acebook_csharp_test";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
            var pass = Environment.GetEnvironmentVariable("DB_PASS") ?? "postgres";

            var cs = $"Host={host};Database={name};Username={user};Password={pass}";
            Console.WriteLine($"🧪 Test DB cs: {cs}");

            var options = new DbContextOptionsBuilder<AcebookDbContext>()
                .UseNpgsql(cs) // no EnableRetryOnFailure for tests
                .Options;

            return new AcebookDbContext(options);
        }
    }
}
