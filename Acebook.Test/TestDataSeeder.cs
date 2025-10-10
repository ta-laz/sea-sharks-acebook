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

		var createdAt = new DateTime(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc);
		var pwHash = "ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f"; // password123

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
				Password = pwHash,
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
	}
}