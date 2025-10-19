using Microsoft.EntityFrameworkCore;
using acebook.Models;

namespace Acebook.TestHelpers
{
    public static class DbFactory
    {
        public static AcebookDbContext CreateTestDb()
        {
            var cs = Environment.GetEnvironmentVariable("DB_NAME");
            if (cs != "acebook_csharp_test")
            {
                Console.WriteLine("⚠️  Dev database.");
                // cs = "Host=localhost;Database=acebook_test;Username=postgres;Password=postgres";
            }
            else
            {
                Console.WriteLine("✅ Using TEST_DATABASE_URL from environment.");
            }

            var options = new DbContextOptionsBuilder<AcebookDbContext>()
                .UseNpgsql(cs)
                .Options;

            return new AcebookDbContext(options);
        }
    }
}