namespace Acebook.Test;
using Microsoft.EntityFrameworkCore;
using acebook.Models;

internal static class TestDataSeeder
{

    public static async Task EnsureDbReadyAsync(AcebookDbContext db)
    {
        // If you use migrations in the app, prefer Migrate() over EnsureCreated()
        await db.Database.EnsureCreatedAsync();
    }

    public static async Task ResetAndSeedAsync(AcebookDbContext db)
    {
        await db.Database.OpenConnectionAsync();
        await db.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE "Likes","Comments","Posts",
                             "Friends","ProfileBios","Users"
            RESTART IDENTITY CASCADE;
        """);

        var createdAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var pwHash = "ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f"; //password123

        // ───────────── USERS ─────────────
        var users = new List<User>
        {
            new() { FirstName="Finn", LastName="White", Email="finn.white@sharkmail.ocean", Password=pwHash },
            new() { FirstName="Shelly", LastName="Tiger", Email="shelly.tiger@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Bruce", LastName="Hammerhead", Email="bruce.hammerhead@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Coral", LastName="Reef", Email="coral.reef@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Reef", LastName="Blue", Email="reef.blue@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Tigra", LastName="Mako", Email="tigra.mako@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Basky", LastName="Bull", Email="basky.bull@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Makko", LastName="Sandbar", Email="makko.sandbar@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Nibbles", LastName="Nurse", Email="nibbles.nurse@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Jet", LastName="Blue", Email="jet.blue@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Tide", LastName="Reef", Email="tide.reef@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Wave", LastName="Lemon", Email="wave.lemon@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Snappy", LastName="Bull", Email="snappy.bull@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Chomp", LastName="White", Email="chomp.white@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Dorsal", LastName="Blue", Email="dorsal.blue@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Shadow", LastName="Tiger", Email="shadow.tiger@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Gilly", LastName="Hammer", Email="gilly.hammer@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Razor", LastName="Mako", Email="razor.mako@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Shelldon", LastName="Reef", Email="shelldon.reef@sharkmail.ocean", Password=pwHash},
            new() { FirstName="Jaws", LastName="Bull", Email="jaws.bull@sharkmail.ocean", Password=pwHash}
        };
        db.AddRange(users);
        await db.SaveChangesAsync();

        // ───────────── BIOS ─────────────
        var bios = new List<ProfileBio>
        {
            new() { UserId = 1,  Tagline = "Explorer of coral reefs and deep thinker.",
                    DOB = new DateOnly(1998, 5, 12), RelationshipStatus = "Single", Job = "Reef Guide", Pets = "Remora" },

            new() { UserId = 2,  Tagline = "Predator of productivity, lover of plankton memes.",
                    DOB = new DateOnly(2001, 2, 28), RelationshipStatus = "In a current", Job = "Content Creator", Pets = "Clownfish" },

            new() { UserId = 3,  Tagline = "Ocean’s apex comedian.",
                    DOB = new DateOnly(1995, 11, 3), RelationshipStatus = "Single", Job = "Stand-up Shark", Pets = "Crab" },

            new() { UserId = 4,  Tagline = "Protecting the reef since hatching.",
                    DOB = new DateOnly(1997, 4, 17), RelationshipStatus = "Married", Job = "Reef Ranger", Pets = "Coral crab" },

            new() { UserId = 5,  Tagline = "Enjoys long swims and short naps.",
                    DOB = new DateOnly(1999, 8, 9), RelationshipStatus = "Single", Job = "Marine Blogger", Pets = "Starfish" },

            new() { UserId = 6,  Tagline = "Never trust a dolphin who promises free mackerel.",
                    DOB = new DateOnly(2000, 3, 25), RelationshipStatus = "It's complicated", Job = "Ocean Philosopher", Pets = "Jellyfish" },

            new() { UserId = 7,  Tagline = "Sharkfluencer and fin-tech investor.",
                    DOB = new DateOnly(1994, 12, 2), RelationshipStatus = "In a current", Job = "FinTech CEO", Pets = "Cleaner wrasse" },

            new() { UserId = 8,  Tagline = "Believes in sustainable snacking.",
                    DOB = new DateOnly(1996, 7, 14), RelationshipStatus = "Married", Job = "Environmental Activist", Pets = "Hermit crab" },

            new() { UserId = 9,  Tagline = "Marine biologist in disguise.",
                    DOB = new DateOnly(1997, 10, 30), RelationshipStatus = "Single", Job = "Research Diver", Pets = "Baby squid" },

            new() { UserId = 10, Tagline = "Swift swimmer, slow texter.",
                    DOB = new DateOnly(1993, 1, 5), RelationshipStatus = "Single", Job = "Courier Shark", Pets = "Seahorse" },

            new() { UserId = 11, Tagline = "Chasing waves, dodging nets.",
                    DOB = new DateOnly(1998, 6, 20), RelationshipStatus = "Single", Job = "Safety Officer", Pets = "Sea cucumber" },

            new() { UserId = 12, Tagline = "Born to bite, raised to blog.",
                    DOB = new DateOnly(2001, 9, 9), RelationshipStatus = "In a current", Job = "Ocean Influencer", Pets = "Pufferfish" },

            new() { UserId = 13, Tagline = "Ocean’s top motivational biter.",
                    DOB = new DateOnly(1995, 11, 12), RelationshipStatus = "Married", Job = "Public Speaker", Pets = "Turtle" },

            new() { UserId = 14, Tagline = "Runs a kelp smoothie bar.",
                    DOB = new DateOnly(1994, 8, 28), RelationshipStatus = "Single", Job = "Bar Owner", Pets = "Kelp crab" },

            new() { UserId = 15, Tagline = "Collects rare shells and followers.",
                    DOB = new DateOnly(1996, 10, 16), RelationshipStatus = "In a current", Job = "Shell Collector", Pets = "Barnacle" },

            new() { UserId = 16, Tagline = "Co-founder of SharkDAO.",
                    DOB = new DateOnly(1997, 12, 21), RelationshipStatus = "Married", Job = "Blockchain Developer", Pets = "Algae snail" },

            new() { UserId = 17, Tagline = "Social swimmer, occasional philosopher.",
                    DOB = new DateOnly(1999, 2, 1), RelationshipStatus = "Single", Job = "Community Manager", Pets = "Remora" },

            new() { UserId = 18, Tagline = "Surfs currents of innovation.",
                    DOB = new DateOnly(2000, 4, 4), RelationshipStatus = "Single", Job = "Product Designer", Pets = "Goby fish" },

            new() { UserId = 19, Tagline = "Part-time model, full-time menace.",
                    DOB = new DateOnly(1998, 11, 27), RelationshipStatus = "In a current", Job = "Model", Pets = "Shrimp" },

            new() { UserId = 20, Tagline = "Always hungry for knowledge (and fish).",
                    DOB = new DateOnly(1993, 3, 15), RelationshipStatus = "Single", Job = "Ocean Researcher", Pets = "Octopus" }
        };

        db.AddRange(bios);
        await db.SaveChangesAsync();

        // ───────────── FRIENDSHIPS ─────────────
        var friends = new (int requester, int accepter, FriendStatus status)[]
        {
            (1, 2, FriendStatus.Accepted),
            (1, 3, FriendStatus.Accepted),
            (2, 4, FriendStatus.Pending),
            (3, 5, FriendStatus.Accepted),
            (4, 5, FriendStatus.Pending),
            (6, 7, FriendStatus.Accepted),
            (6, 8, FriendStatus.Pending),
            (7, 9, FriendStatus.Accepted),
            (10, 11, FriendStatus.Accepted),
            (10, 12, FriendStatus.Pending),
            (11, 13, FriendStatus.Accepted),
            (12, 14, FriendStatus.Accepted),
            (15, 16, FriendStatus.Accepted),
            (15, 17, FriendStatus.Pending),
            (16, 18, FriendStatus.Accepted),
            (17, 19, FriendStatus.Accepted),
            (18, 20, FriendStatus.Pending),
            (1, 10, FriendStatus.Accepted),
            (5, 15, FriendStatus.Accepted)
        };

        foreach (var (r, a, status) in friends)
        {
            db.Add(new Friend
            {
                RequesterId = r,
                AccepterId = a,
                Status = status,
            });
        }
        await db.SaveChangesAsync();

        // ───────────── POSTS ─────────────
        var posts = new List<Post>
        {
            // self-posts
            new() { UserId = 1, WallId = 1, Content = "Just circled the reef with @Shelly Tiger — great current today!", CreatedOn = createdAt },
            new() { UserId = 2, WallId = 2, Content = "Early morning hunt with Bruce Hammerhead. The ocean was ours.", CreatedOn = createdAt },
            new() { UserId = 3, WallId = 3, Content = "Every wave is an opportunity to reflect… or to eat.", CreatedOn = createdAt },
            new() { UserId = 4, WallId = 4, Content = "Grateful for my coral friends. @Reef Blue keeps me inspired.", CreatedOn = createdAt },

            // cross-wall posts (only if they’re friends)
            new() { UserId = 1, WallId = 2, Content = "Dropped by @Shelly Tiger’s reef to say hi!", CreatedOn = createdAt },
            new() { UserId = 2, WallId = 1, Content = "Appreciate @Finn White for helping find tuna today!", CreatedOn = createdAt },
            new() { UserId = 3, WallId = 5, Content = "@Reef Blue — let’s patrol the western trench soon!", CreatedOn = createdAt },
            new() { UserId = 6, WallId = 7, Content = "Always a pleasure working with @Basky Bull on reef patrol!", CreatedOn = createdAt },
            new() { UserId = 7, WallId = 8, Content = "Teamwork makes the fin work @Makko Sandbar!", CreatedOn = createdAt },
            new() { UserId = 10, WallId = 1, Content = "Visiting @Finn White’s kelp bar — great service!", CreatedOn = createdAt },
        };

        db.AddRange(posts);
        await db.SaveChangesAsync();

        // ───────────── COMMENTS + LIKES ─────────────
        db.AddRange(
            new Comment { UserId = 2, PostId = 1, Content = "Fin-tastic swim!", CreatedOn = createdAt },
            new Comment { UserId = 3, PostId = 2, Content = "Legendary hunt!", CreatedOn = createdAt },
            new Comment { UserId = 5, PostId = 4, Content = "Beautifully said.", CreatedOn = createdAt },
            new Comment { UserId = 10, PostId = 1, Content = "Love this energy!", CreatedOn = createdAt },
            new Like { UserId = 3, PostId = 1},
            new Like { UserId = 4, PostId = 2},
            new Like { UserId = 1, PostId = 5},
            new Like { UserId = 8, PostId = 7}
        );
        await db.SaveChangesAsync();
    }
}