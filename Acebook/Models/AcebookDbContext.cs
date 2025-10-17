namespace acebook.Models;

using Microsoft.EntityFrameworkCore;

public class AcebookDbContext : DbContext
{
  public DbSet<Post>? Posts { get; set; }
  public DbSet<User>? Users { get; set; }
  public DbSet<ProfileBio>? ProfileBios { get; set; }
  public DbSet<Like>? Likes { get; set; }
  public DbSet<Comment>? Comments { get; set; }
  public DbSet<Friend>? Friends { get; set; }


  public string? DbPath { get; }

  public string? GetDatabaseName()
  {
    string? DatabaseNameArg = Environment.GetEnvironmentVariable("DATABASE_NAME");

    if (DatabaseNameArg == null)
    {
      System.Console.WriteLine(
        "DATABASE_NAME is null. Defaulting to test database."
      );
      return "acebook_csharp_test";
    }
    else
    {
      System.Console.WriteLine(
        "Connecting to " + DatabaseNameArg
      );
      return DatabaseNameArg;
    }
  }

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    // Check if a connection string is provided via environment variable
    var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

    if (!string.IsNullOrEmpty(connStr))
    {
        optionsBuilder.UseNpgsql(connStr);
    }
    else
    {
        // Fallback to your current local logic
        optionsBuilder.UseNpgsql(@"Host=localhost;Username=postgres;Password=1234;Database=" + GetDatabaseName());
    }
}


  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Post>()
      .Navigation(post => post.User)
      .AutoInclude();

    modelBuilder.Entity<Friend>()
            .HasOne(f => f.Requester)
            .WithMany(u => u.FriendRequestsSent)
            .HasForeignKey(f => f.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Friend>()
            .HasOne(f => f.Accepter)
            .WithMany(u => u.FriendRequestsReceived)
            .HasForeignKey(f => f.AccepterId)
            .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Like>() //Database logic to check that a like can only belong to a comment OR a post not both in the same instance
                .HasCheckConstraint("Check_Likes_Only_One",
                "(\"PostId\" IS NOT NULL AND \"CommentId\" IS NULL) OR (\"PostId\" IS NULL AND \"CommentId\" IS NOT NULL)");
    modelBuilder.Entity<Like>()
            .HasIndex(l => new { l.UserId, l.PostId })
            .IsUnique();

    modelBuilder.Entity<Like>()
            .HasIndex(l => new { l.UserId, l.CommentId })
            .IsUnique();

    // When a Post is deleted -> delete its Comments
    modelBuilder.Entity<Post>()
        .HasMany(p => p.Comments)
        .WithOne(c => c.Post)
        .HasForeignKey(c => c.PostId)
        .OnDelete(DeleteBehavior.Cascade);

    // When a Post is deleted -> delete its Likes
    modelBuilder.Entity<Post>()
        .HasMany(p => p.Likes)
        .WithOne(l => l.Post)
        .HasForeignKey(l => l.PostId)
        .OnDelete(DeleteBehavior.Cascade);

    // When a Comment is deleted -> delete its Likes
    modelBuilder.Entity<Comment>()
        .HasMany(c => c.Likes)
        .WithOne(l => l.Comment)
        .HasForeignKey(l => l.CommentId)
        .OnDelete(DeleteBehavior.Cascade);

  }
}
