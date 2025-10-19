namespace acebook.Models
{
    using Microsoft.EntityFrameworkCore;

    public class AcebookDbContext : DbContext
    {
        public AcebookDbContext(DbContextOptions<AcebookDbContext> options)
            : base(options) { }

        public DbSet<Post>? Posts { get; set; }
        public DbSet<User>? Users { get; set; }
        public DbSet<ProfileBio>? ProfileBios { get; set; }
        public DbSet<Like>? Likes { get; set; }
        public DbSet<Comment>? Comments { get; set; }
        public DbSet<Friend>? Friends { get; set; }
        public DbSet<Notification> Notifications { get; set; }

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

            modelBuilder.Entity<Like>()
                .HasCheckConstraint("Check_Likes_Only_One",
                    "(\"PostId\" IS NOT NULL AND \"CommentId\" IS NULL) OR (\"PostId\" IS NULL AND \"CommentId\" IS NOT NULL)");

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.PostId })
                .IsUnique();

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.CommentId })
                .IsUnique();

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Likes)
                .WithOne(l => l.Post)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasMany(c => c.Likes)
                .WithOne(l => l.Comment)
                .HasForeignKey(l => l.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Sender)
                .WithMany(u => u.SentNotifications)
                .HasForeignKey(n => n.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne<User>()
                .WithMany(u => u.ReceivedNotifications)
                .HasForeignKey(n => n.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}