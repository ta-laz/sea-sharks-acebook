using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;
using acebook.Models; // ✅ add this so EF sees your DbContext class

public class AcebookDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AcebookDbContext>
{
    public AcebookDbContext CreateDbContext(string[] args)
    {
        // Try dev env vars; fall back to a sensible local default
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var name = Environment.GetEnvironmentVariable("DB_NAME") ?? "acebook_dev";
        var user = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var pass = Environment.GetEnvironmentVariable("DB_PASS") ?? "postgres";

        var cs = $"Host={host};Database={name};Username={user};Password={pass}";

        var options = new DbContextOptionsBuilder<AcebookDbContext>()
            .UseNpgsql(cs)
            .Options;

        return new AcebookDbContext(options);
    }
}