namespace Acebook.Test;

using Microsoft.EntityFrameworkCore;
using acebook.Models;
using Microsoft.AspNetCore.Identity;

internal static class TestDataSeeder
{
    private static readonly PasswordHasher<User> Hasher = new();
    private static readonly string PwHash = Hasher.HashPassword(new User(), "password123");

    public static async Task EnsureDbReadyAsync(AcebookDbContext db)
	{
		// If you maintain migrations, prefer Migrate over EnsureCreated:
		Console.WriteLine("🔧 Ensuring database is ready...");
		await db.Database.MigrateAsync();
		Console.WriteLine("✅ Database schema ready.");
        // If you're *not* using migrations for tests, keep:
        // await db.Database.EnsureCreatedAsync();
    }
    public static async Task ResetAndSeedAsync(AcebookDbContext db)
	{
		Console.WriteLine("🧹 Resetting and reseeding Acebook test database...");
		await db.Database.OpenConnectionAsync();
        await using var tx = await db.Database.BeginTransactionAsync();
		try
		{
			await db.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE "Likes","Comments","Posts",
                             "Friends","ProfileBios","Users"
            RESTART IDENTITY CASCADE;
        """);

			var createdAt = new DateTime(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc);


			// var pwHash = "ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f";

			// ---------- USERS (50) ----------
			// deterministic pools we cycle through
			string[] firsts = {
		"Finn","Shelly","Bruce","Coral","Reef","Tigra","Basky","Makko","Nibbles","Jet",
		"Tide","Wave","Snappy","Chomp","Dorsal","Shadow","Gilly","Razor","Shelldon","Jaws",
		"Marina","Ripple","Echo","Krill","Torrent","Foam","Gust","Brine","Flint","Delta",
		"Current","Breaker","Ridge","Slate","Fathom","Drift","Gale","Squall","Coraline","Pearl",
		"Nerite","Onyx","Azure","Cobalt","Indigo","Teal","Cerulean","Harbor","Reefy","Bluey"
	};
			string[] lasts = { "White", "Tiger", "Hammerhead", "Reef", "Blue", "Mako", "Bull", "Sandbar", "Nurse", "Lemon" };
			string[] profilePics = { null, "/images/profile_pics/233aff90-5548-4fc5-9d09-ab293ef52db1.png", "/images/profile_pics/15c34476-5803-431b-968c-31c1ec391063.png", "/images/profile_pics/5e97e964-1320-4067-a159-9f8835179098.png", "/images/profile_pics/708c093e-df72-4e92-bf09-ccd6001949b2.jpg", "/images/profile_pics/e2d18531-0497-47b7-b593-9066f7c09d62.png", "/images/profile_pics/5eda5f7a-83b0-47f3-8461-63115fd3eed0.png", "/images/profile_pics/3485778a-755f-4172-9a20-cbba83acd63d.png", "/images/profile_pics/e488266c-22b5-44c6-a83c-c043ddcd03ab.png", "/images/profile_pics/a2fa1a1b-f815-4e75-a60f-7aa190611c53.jpg", "/images/profile_pics/f2749ee6-0738-4357-9fc6-6c55646c70c8.png", "/images/profile_pics/098c2f17-bdfe-4f9d-8a54-483894dd741e.png", "/images/profile_pics/230a8047-4a18-491a-b8ea-95125cdb414b.png", "/images/profile_pics/4e205f1a-2873-4d0e-a188-c0102e21bde4.png", "/images/profile_pics/6e1b30fe-290a-4fa3-b69d-8cf9dba8747d.png", "/images/profile_pics/394cf5a6-bc17-4191-8ce9-a7fa4fdbbef4.png", "/images/profile_pics/d23c1119-3ba7-4ebb-9894-a6ff93d7bae1.png", "/images/profile_pics/6014c215-3fc6-43f6-a2c1-81d52c685a78.png", "/images/profile_pics/5c9ef11e-c5a4-4f15-9c71-16d569233d48.png", "/images/profile_pics/8c10caf7-f982-4cf0-970a-00d04e5a2853.jpg", "/images/profile_pics/3b446ddd-8e79-4d63-afef-9eade537a4e0.jpg", "/images/profile_pics/e0a1125c-e9c4-40da-8f90-302090cbe072.jpg", "/images/profile_pics/2a2715b3-3244-44e2-af55-6cc7aa6acad2.jpg", "/images/profile_pics/bf94767e-7964-466f-840f-4d56aa5f42bf.jpg", "/images/profile_pics/94566dc2-6ce1-4437-b907-76051e53cb97.jpg", "/images/profile_pics/870d2fcf-be67-4fae-ab53-c8eff646e9a2.jpg", "/images/profile_pics/ef691073-e3f8-4dce-bdf6-a25974cfdcd8.jpg", "/images/profile_pics/c03a6207-3191-4693-a5d1-4099b86c9941.jpg", "/images/profile_pics/39ef174e-b927-41cf-bb05-f986d3c0b472.jpg", "/images/profile_pics/eafc4eaf-e1de-4bae-91c8-f7ad61249c51.jpg", "/images/profile_pics/8de3a883-30df-4f4f-8b4f-c032e8a5931c.jpg", "/images/profile_pics/5eba3523-3326-4a9f-ad0c-d3ff9aa9b138.jpg", "/images/profile_pics/c6a03020-e53c-46a0-93b7-f1ec50aac42c.jpg", "/images/profile_pics/3094a176-f517-4da3-acc3-4c941028fdd0.jpg", "/images/profile_pics/b154dc29-f00d-4a14-8238-95df539540e3.jpg", "/images/profile_pics/fd0eb600-d83b-4337-9a15-670bf9dda708.png", "/images/profile_pics/50de858d-5d15-485f-ab2c-c01ce945948c.png", "/images/profile_pics/cfb1d583-24ac-4c11-aa9e-3cb6ffd95932.png", "/images/profile_pics/8b1ca221-866f-4043-939f-1725e0d4e74d.png", "/images/profile_pics/105cbbc5-eade-4455-9362-8a0a00af8e24.png", "/images/profile_pics/7b0233be-eaeb-4604-afe4-2c4baba49a62.png", "/images/profile_pics/b9c5befd-70df-42d8-baad-2230e0b708d5.png", "/images/profile_pics/b11abf18-0146-4aff-bc6e-a2d51bf792d1.png", "/images/profile_pics/1537620f-e9d9-42b6-8df8-6ac15732ae3c.png", "/images/profile_pics/6fdcbcbe-885b-4c2b-b4e8-84ec01f9bc9e.png", "/images/profile_pics/b94b608c-5a19-4dc7-8281-bcd610e756e0.png", "/images/profile_pics/4cb8a41f-99b7-4c9e-ba6c-61f9ab9e209c.png", "/images/profile_pics/d8b4dc69-d659-406b-a22e-2c51b4c251b6.png", "/images/profile_pics/5586e5e3-bbbb-47e0-a144-cd1877dc4612.png", "/images/profile_pics/d034287a-6243-4402-9398-d5de8cfef800.png" };
			var users = new List<User>(50);
			for (int i = 0; i < 50; i++)
			{
				var f = firsts[i % firsts.Length];
				var l = lasts[i % lasts.Length];
				users.Add(new User
				{
					FirstName = f,
					LastName = l,
					Email = $"{f.ToLower()}.{l.ToLower()}@sharkmail.ocean",
					Password = PwHash,
					ProfilePicturePath = profilePics[i]
					// If your model has CreatedAt, uncomment:
					// CreatedAt = createdAt.AddMinutes(i * 7)
				});
			}
			db.AddRange(users);
			await db.SaveChangesAsync();

			// ---------- BIOS (Tagline, DOB, RelationshipStatus, Job, Pets) ----------
			string[] taglines = {
		"Explorer of coral reefs and deep thinker.",
		"Predator of productivity, lover of plankton memes.",
		"Ocean’s apex comedian.",
		"Protecting the reef since hatching.",
		"Enjoys long swims and short naps.",
		"Never trust a dolphin who promises free mackerel.",
		"Sharkfluencer and fin-tech investor.",
		"Believes in sustainable snacking.",
		"Marine biologist in disguise.",
		"Swift swimmer, slow texter.",
		"Chasing waves, dodging nets.",
		"Born to bite, raised to blog.",
		"Ocean’s top motivational biter.",
		"Runs a kelp smoothie bar.",
		"Collects rare shells and followers.",
		"Co-founder of SharkDAO.",
		"Social swimmer, occasional philosopher.",
		"Surfs currents of innovation.",
		"Part-time model, full-time menace.",
		"Always hungry for knowledge (and fish)."
	};
			string[] rels = { "Single", "In a current", "Married", "It's complicated" };
			string[] jobs = {
		"Reef Guide","Content Creator","Stand-up Shark","Reef Ranger","Marine Blogger",
		"Ocean Philosopher","FinTech CEO","Environmental Activist","Research Diver","Courier Shark"
	};
			string[] pets = {
		"Remora","Clownfish","Crab","Coral crab","Starfish","Jellyfish","Cleaner wrasse","Hermit crab","Baby squid","Seahorse",
		"Sea cucumber","Pufferfish","Turtle","Kelp crab","Barnacle","Algae snail","Remora","Goby fish","Shrimp","Octopus"
	};

			var bios = new List<ProfileBio>(50);
			for (int i = 0; i < 50; i++)
			{
				// Deterministic DOBs spread across years/months/days
				var dob = new DateOnly(
					1993 + ((i * 7) % 10),
					1 + ((i * 3) % 12),
					1 + ((i * 5) % 28)
				);
				bios.Add(new ProfileBio
				{
					UserId = i + 1,
					Tagline = taglines[i % taglines.Length],
					DOB = dob,
					RelationshipStatus = rels[i % rels.Length],
					Job = jobs[i % jobs.Length],
					Pets = pets[i % pets.Length]
				});
			}
			db.AddRange(bios);
			await db.SaveChangesAsync();

			// ---------- FRIENDSHIPS ----------
			// For each user i (1..50):
			// - Accepted with i+1 and i+2 (wrap)
			// - Pending SENT to i+3 and i+4
			// - Pending RECEIVED ensured by adding (i-3)->i and (i-4)->i
			var friends = new List<Friend>();
			int N = 50;

			int Next(int i, int k) => ((i - 1 + k) % N) + 1;
			int Prev(int i, int k) => ((i - 1 - k + N * 10) % N) + 1;

			// Accepted
			for (int i = 1; i <= N; i++)
			{
				var a = Next(i, 1);
				var b = Next(i, 2);
				// normalise direction for Accepted to avoid duplicates
				int r1 = Math.Min(i, a), c1 = Math.Max(i, a);
				int r2 = Math.Min(i, b), c2 = Math.Max(i, b);

				friends.Add(new Friend { RequesterId = r1, AccepterId = c1, Status = FriendStatus.Accepted });
				friends.Add(new Friend { RequesterId = r2, AccepterId = c2, Status = FriendStatus.Accepted });
			}

			// Pending sent
			for (int i = 1; i <= N; i++)
			{
				var c = Next(i, 3);
				var d = Next(i, 4);
				friends.Add(new Friend { RequesterId = i, AccepterId = c, Status = FriendStatus.Pending });
				friends.Add(new Friend { RequesterId = i, AccepterId = d, Status = FriendStatus.Pending });
			}

			// Pending received (ensure everyone has inbound too)
			for (int i = 1; i <= N; i++)
			{
				var e = Prev(i, 3);
				var f = Prev(i, 4);
				friends.Add(new Friend { RequesterId = e, AccepterId = i, Status = FriendStatus.Pending });
				friends.Add(new Friend { RequesterId = f, AccepterId = i, Status = FriendStatus.Pending });
			}

			// Deduplicate any accidental duplicates (especially Accepted pairs)
			var dedup = new HashSet<(int, int, FriendStatus)>();
			var finalFriends = new List<Friend>(friends.Count);
			foreach (var fr in friends)
			{
				var key = fr.Status == FriendStatus.Accepted
					? (Math.Min(fr.RequesterId, fr.AccepterId), Math.Max(fr.RequesterId, fr.AccepterId), fr.Status)
					: (fr.RequesterId, fr.AccepterId, fr.Status);

				if (dedup.Add(key)) finalFriends.Add(fr);
			}

			db.AddRange(finalFriends);
			await db.SaveChangesAsync();

			// ---------- POSTS ----------
			// Each user: 3–4 posts (deterministic), half self-wall, half on friend (i+1)'s wall.
			string[] postStems = {
		"Circling the reef at dawn; currents strong and spirits high.",
		"Training for the trench sprint; fins burning, heart soaring.",
		"Kelp bar opens tonight; try the brine latte, extra foam.",
		"Dolphins told jokes again; I laughed, then ate lunch.",
		"Sardine school shimmering like stars beneath the waves.",
		"Cleaning the reef with friends; teamwork makes the fin work.",
		"Deep thoughts near the canyon; silence louder than surf.",
		"Bubble party at Coral Bay; bring your best ripple dance.",
		"Tide pulled me west; found calm behind the ridge.",
		"Shoutout to crew keeping the nurseries safe and bright."
	};

			var posts = new List<Post>(N * 4);
			int postIdCounter = 0;

			for (int i = 1; i <= N; i++)
			{
				var friend = Next(i, 1); // deterministic friend target
				int postCount = 3 + (i % 2); // 3 or 4

				for (int ord = 0; ord < postCount; ord++)
				{
					bool self = (ord % 2 == 0);
					int author = i;
					int wall = self ? i : friend;

					// mention the wall owner when posting on their wall
					var mention = self ? "" : $" Shoutout to @{firsts[(friend - 1) % firsts.Length]} {lasts[(friend - 1) % lasts.Length]}!";
					var stem = postStems[(i + ord) % postStems.Length];

					// expand to ~10–40 words by appending fixed filler deterministically
					string[] filler = { "swim", "fin", "deep", "blue", "reef", "hunt", "glide", "kelp", "wave", "tide", "current", "school" };
					int extra = 10 + ((i + ord) % 31); // 10..40 words
					var extraText = string.Join(' ', filler.Take(extra % filler.Length).Concat(filler).Take(extra));

					posts.Add(new Post
					{
						UserId = author,
						WallId = wall,
						Content = $"{stem}{mention} {extraText}",
						CreatedOn = createdAt.AddMinutes(2000 + i * 5 + ord) // varied & deterministic
					});
					postIdCounter++;
				}
			}

			db.AddRange(posts);
			await db.SaveChangesAsync();

			// ---------- COMMENTS ----------
			// Most posts (4/5) get 2–3 comments from friends of the author (deterministic pick).
			var comments = new List<Comment>(postIdCounter * 2);
			int globalPostIndex = 0;

			for (int i = 1; i <= N; i++)
			{
				int postCount = 3 + (i % 2);
				var friendA = Next(i, 1);
				var friendB = Next(i, 2);

				for (int ord = 0; ord < postCount; ord++, globalPostIndex++)
				{
					int thisPostId = globalPostIndex + 1; // relies on insertion order
														  // 1 in 5 posts gets no comments
					if (thisPostId % 5 == 0) continue;

					int baseMinute = 3000 + i * 5 + ord * 3;

					// always at least 2 comments
					comments.Add(new Comment
					{
						UserId = friendA,
						PostId = thisPostId,
						Content = "Fin-tastic update — see you by the ridge!",
						CreatedOn = createdAt.AddMinutes(baseMinute)
					});
					comments.Add(new Comment
					{
						UserId = friendB,
						PostId = thisPostId,
						Content = "Currents were wild! Great patrol today.",
						CreatedOn = createdAt.AddMinutes(baseMinute + 1)
					});

					// sometimes add a 3rd comment (deterministic)
					if ((thisPostId % 2) == 0)
					{
						int friendC = Next(i, 3);
						comments.Add(new Comment
						{
							UserId = friendC,
							PostId = thisPostId,
							Content = "Save me a kelp latte next time.",
							CreatedOn = createdAt.AddMinutes(baseMinute + 2)
						});
					}
				}
			}

			db.AddRange(comments);
			await db.SaveChangesAsync();

			// ---------- LIKES ----------
			// Every post gets at least one like, deterministically.
			// UserId chosen as (PostId % totalUsers) + 1 to stay in range.

			var likes = new List<Like>();

			int totalUsers = 50;            // since you seed 50 users
			int totalPosts = posts.Count;   // your earlier posts list

			for (int i = 0; i < totalPosts; i++)
			{
				var post = posts[i];
				int postId = i + 1; // matches DB identity order
				int baseLiker = (postId % totalUsers) + 1; // deterministic liker id

				// --- at least one like ---
				likes.Add(new Like
				{
					UserId = baseLiker,
					PostId = postId

				});

				// --- give every 3rd post a 2nd like (also deterministic) ---
				if (postId % 3 == 0)
				{
					int secondLiker = ((baseLiker + 7) % totalUsers) + 1; // fixed offset
					if (secondLiker != baseLiker)
					{
						likes.Add(new Like
						{
							UserId = secondLiker,
							PostId = postId

						});
					}
				}

				// --- give every 5th post a 3rd like (predictable pattern) ---
				if (postId % 5 == 0)
				{
					int thirdLiker = ((baseLiker + 14) % totalUsers) + 1;
					if (thirdLiker != baseLiker)
					{
						likes.Add(new Like
						{
							UserId = thirdLiker,
							PostId = postId

						});
					}
				}
			}

			db.AddRange(likes);
			await db.SaveChangesAsync();
		
		await tx.CommitAsync();
		Console.WriteLine("✅ Database reseeded successfully.");
		}
		catch (Exception ex)
        {
            Console.WriteLine($"❌ Reseed failed: {ex.Message}");
            await tx.RollbackAsync();
            throw;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
	}
	
}