-- ===========================================
-- Acebook DEMO SEED (hardcoded, no loops)
-- 10 users, bios, ring friendships,
-- 30 posts (3/user), 40 comments (4/user),
-- 40 post-likes (4/user), 50 comment-likes (5/user)
-- Rule enforced: cross-wall posts only to friends
-- ===========================================

TRUNCATE TABLE "Likes","Comments","Posts","Friends","ProfileBios","Users"
RESTART IDENTITY CASCADE;

-- ---------- USERS ----------
-- Password hash = password123 (SHA-256 as in your seed)
WITH u(first,last,email,pic,pw) AS (
  VALUES
  ('Finn','White','finn.white@sharkmail.ocean',NULL,'AQAAAAIAAYagAAAAEK9QJ+6vW6Nf55oWUXtBicImJViR5xGGYF9UH3U6sA/XPZLtmq/Z5M799Ip6NSeOqw=='),
  ('Shelly','Tiger','shelly.tiger@sharkmail.ocean','/images/profile_pics/233aff90-5548-4fc5-9d09-ab293ef52db1.png','AQAAAAIAAYagAAAAEK9QJ+6vW6Nf55oWUXtBicImJViR5xGGYF9UH3U6sA/XPZLtmq/Z5M799Ip6NSeOqw=='),
  ('Bruce','Hammerhead','bruce.hammerhead@sharkmail.ocean','/images/profile_pics/15c34476-5803-431b-968c-31c1ec391063.png','AQAAAAIAAYagAAAAEK9QJ+6vW6Nf55oWUXtBicImJViR5xGGYF9UH3U6sA/XPZLtmq/Z5M799Ip6NSeOqw=='),
  ('Coral','Reef','coral.reef@sharkmail.ocean','/images/profile_pics/5e97e964-1320-4067-a159-9f8835179098.png','AQAAAAIAAYagAAAAEK9QJ+6vW6Nf55oWUXtBicImJViR5xGGYF9UH3U6sA/XPZLtmq/Z5M799Ip6NSeOqw=='),
  ('Reef','Blue','reef.blue@sharkmail.ocean','/images/profile_pics/708c093e-df72-4e92-bf09-ccd6001949b2.jpg','AQAAAAIAAYagAAAAEK9QJ+6vW6Nf55oWUXtBicImJViR5xGGYF9UH3U6sA/XPZLtmq/Z5M799Ip6NSeOqw=='),
  ('Tigra','Mako','tigra.mako@sharkmail.ocean','/images/profile_pics/e2d18531-0497-47b7-b593-9066f7c09d62.png','AQAAAAIAAYagAAAAEK9QJ+6vW6Nf55oWUXtBicImJViR5xGGYF9UH3U6sA/XPZLtmq/Z5M799Ip6NSeOqw=='),
  ('Basky','Bull','basky.bull@sharkmail.ocean','/images/profile_pics/5eda5f7a-83b0-47f3-8461-63115fd3eed0.png','AQAAAAIAAYagAAAAEK9QJ+6vW6Nf55oWUXtBicImJViR5xGGYF9UH3U6sA/XPZLtmq/Z5M799Ip6NSeOqw=='),
  ('Makko','Sandbar','makko.sandbar@sharkmail.ocean','/images/profile_pics/3485778a-755f-4172-9a20-cbba83acd63d.png','AQAAAAIAAYagAAAAEK9QJ+6vW6Nf55oWUXtBicImJViR5xGGYF9UH3U6sA/XPZLtmq/Z5M799Ip6NSeOqw=='),
  ('Nibbles','Nurse','nibbles.nurse@sharkmail.ocean','/images/profile_pics/e488266c-22b5-44c6-a83c-c043ddcd03ab.png','AQAAAAIAAYagAAAAEK9QJ+6vW6Nf55oWUXtBicImJViR5xGGYF9UH3U6sA/XPZLtmq/Z5M799Ip6NSeOqw=='),
  ('Jet','Lemon','jet.lemon@sharkmail.ocean','/images/profile_pics/a2fa1a1b-f815-4e75-a60f-7aa190611c53.jpg','AQAAAAIAAYagAAAAEK9QJ+6vW6Nf55oWUXtBicImJViR5xGGYF9UH3U6sA/XPZLtmq/Z5M799Ip6NSeOqw==')
)
INSERT INTO "Users" ("FirstName","LastName","Email","Password","ProfilePicturePath")
SELECT first,last,email,pw,pic FROM u;

-- ---------- BIOS ----------
WITH b(email,tagline,dob,rel,job,pets) AS (
  VALUES
  ('finn.white@sharkmail.ocean','Explorer of coral reefs and deep thinker.','1995-02-05','Single','Reef Guide','Remora'),
  ('shelly.tiger@sharkmail.ocean','Predator of productivity, lover of plankton memes.','1994-05-06','In a current','Content Creator','Clownfish'),
  ('bruce.hammerhead@sharkmail.ocean','Ocean’s apex comedian.','1996-08-07','Married','Stand-up Shark','Crab'),
  ('coral.reef@sharkmail.ocean','Protecting the reef since hatching.','1993-03-08','It''s complicated','Reef Ranger','Starfish'),
  ('reef.blue@sharkmail.ocean','Enjoys long swims and short naps.','1997-06-09','Single','Marine Blogger','Jellyfish'),
  ('tigra.mako@sharkmail.ocean','Never trust a dolphin who promises free mackerel.','1994-09-10','In a current','Ocean Philosopher','Cleaner wrasse'),
  ('basky.bull@sharkmail.ocean','Sharkfluencer and fin-tech investor.','1998-12-11','Married','FinTech CEO','Hermit crab'),
  ('makko.sandbar@sharkmail.ocean','Believes in sustainable snacking.','1995-01-12','It''s complicated','Environmental Activist','Baby squid'),
  ('nibbles.nurse@sharkmail.ocean','Marine biologist in disguise.','1996-04-13','Single','Research Diver','Seahorse'),
  ('jet.lemon@sharkmail.ocean','Swift swimmer, slow texter.','1993-07-14','In a current','Courier Shark','Sea cucumber')
)
INSERT INTO "ProfileBios" ("UserId","Tagline","DOB","RelationshipStatus","Job","Pets")
SELECT u."Id", b.tagline, b.dob::date, b.rel, b.job, b.pets
FROM b JOIN "Users" u ON u."Email" = b.email;

-- ---------- FRIENDSHIPS (ring: each user has 2 friends) ----------
WITH f(a_email,b_email) AS (
  VALUES
  ('finn.white@sharkmail.ocean','shelly.tiger@sharkmail.ocean'),
  ('shelly.tiger@sharkmail.ocean','bruce.hammerhead@sharkmail.ocean'),
  ('bruce.hammerhead@sharkmail.ocean','coral.reef@sharkmail.ocean'),
  ('coral.reef@sharkmail.ocean','reef.blue@sharkmail.ocean'),
  ('reef.blue@sharkmail.ocean','tigra.mako@sharkmail.ocean'),
  ('tigra.mako@sharkmail.ocean','basky.bull@sharkmail.ocean'),
  ('basky.bull@sharkmail.ocean','makko.sandbar@sharkmail.ocean'),
  ('makko.sandbar@sharkmail.ocean','nibbles.nurse@sharkmail.ocean'),
  ('nibbles.nurse@sharkmail.ocean','jet.lemon@sharkmail.ocean'),
  ('jet.lemon@sharkmail.ocean','finn.white@sharkmail.ocean')
)
INSERT INTO "Friends" ("RequesterId","AccepterId","Status")
SELECT LEAST(u1."Id",u2."Id"), GREATEST(u1."Id",u2."Id"), 1
FROM f
JOIN "Users" u1 ON u1."Email" = f.a_email
JOIN "Users" u2 ON u2."Email" = f.b_email;

-- ---------- POSTS (3 per user; #2 goes to next friend’s wall)
-- We add a unique tag [POST:<email>#n] to make comments/likes targetable without ids.
WITH p(author,wall,content,created_on) AS (
  VALUES
  -- Finn
  ('finn.white@sharkmail.ocean','finn.white@sharkmail.ocean','Circling the reef at dawn. [POST:finn.white#1]','2025-01-01 08:10:00+00'),
  ('finn.white@sharkmail.ocean','shelly.tiger@sharkmail.ocean','Training for the trench sprint. Shoutout @Shelly! [POST:finn.white#2]','2025-01-01 08:20:00+00'),
  ('finn.white@sharkmail.ocean','finn.white@sharkmail.ocean','Kelp bar opens tonight. [POST:finn.white#3]','2025-01-01 08:30:00+00'),

  -- Shelly
  ('shelly.tiger@sharkmail.ocean','shelly.tiger@sharkmail.ocean','Dolphins told jokes again. [POST:shelly.tiger#1]','2025-01-01 08:40:00+00'),
  ('shelly.tiger@sharkmail.ocean','bruce.hammerhead@sharkmail.ocean','Sardine school shimmering. Shoutout @Bruce! [POST:shelly.tiger#2]','2025-01-01 08:50:00+00'),
  ('shelly.tiger@sharkmail.ocean','shelly.tiger@sharkmail.ocean','Cleaning the reef with friends. [POST:shelly.tiger#3]','2025-01-01 09:00:00+00'),

  -- Bruce
  ('bruce.hammerhead@sharkmail.ocean','bruce.hammerhead@sharkmail.ocean','Deep thoughts near the canyon. [POST:bruce.hammerhead#1]','2025-01-01 09:10:00+00'),
  ('bruce.hammerhead@sharkmail.ocean','coral.reef@sharkmail.ocean','Bubble party at Coral Bay. @Coral! [POST:bruce.hammerhead#2]','2025-01-01 09:20:00+00'),
  ('bruce.hammerhead@sharkmail.ocean','bruce.hammerhead@sharkmail.ocean','Tide pulled me west. [POST:bruce.hammerhead#3]','2025-01-01 09:30:00+00'),

  -- Coral
  ('coral.reef@sharkmail.ocean','coral.reef@sharkmail.ocean','Shoutout to nursery crew. [POST:coral.reef#1]','2025-01-01 09:40:00+00'),
  ('coral.reef@sharkmail.ocean','reef.blue@sharkmail.ocean','Currents were wild, @Reef! [POST:coral.reef#2]','2025-01-01 09:50:00+00'),
  ('coral.reef@sharkmail.ocean','coral.reef@sharkmail.ocean','Gliding over the ridge. [POST:coral.reef#3]','2025-01-01 10:00:00+00'),

  -- Reef
  ('reef.blue@sharkmail.ocean','reef.blue@sharkmail.ocean','Fin burning, heart soaring. [POST:reef.blue#1]','2025-01-01 10:10:00+00'),
  ('reef.blue@sharkmail.ocean','tigra.mako@sharkmail.ocean','Reef patrol with @Tigra. [POST:reef.blue#2]','2025-01-01 10:20:00+00'),
  ('reef.blue@sharkmail.ocean','reef.blue@sharkmail.ocean','Kelp latte secured. [POST:reef.blue#3]','2025-01-01 10:30:00+00'),

  -- Tigra
  ('tigra.mako@sharkmail.ocean','tigra.mako@sharkmail.ocean','Training sprints logged. [POST:tigra.mako#1]','2025-01-01 10:40:00+00'),
  ('tigra.mako@sharkmail.ocean','basky.bull@sharkmail.ocean','Teamwork w/ @Basky. [POST:tigra.mako#2]','2025-01-01 10:50:00+00'),
  ('tigra.mako@sharkmail.ocean','tigra.mako@sharkmail.ocean','Blue waters today. [POST:tigra.mako#3]','2025-01-01 11:00:00+00'),

  -- Basky
  ('basky.bull@sharkmail.ocean','basky.bull@sharkmail.ocean','Canyon silence hits. [POST:basky.bull#1]','2025-01-01 11:10:00+00'),
  ('basky.bull@sharkmail.ocean','makko.sandbar@sharkmail.ocean','Ripple dance soon, @Makko. [POST:basky.bull#2]','2025-01-01 11:20:00+00'),
  ('basky.bull@sharkmail.ocean','basky.bull@sharkmail.ocean','Shimmering sardines. [POST:basky.bull#3]','2025-01-01 11:30:00+00'),

  -- Makko
  ('makko.sandbar@sharkmail.ocean','makko.sandbar@sharkmail.ocean','Cobalt morning swim. [POST:makko.sandbar#1]','2025-01-01 11:40:00+00'),
  ('makko.sandbar@sharkmail.ocean','nibbles.nurse@sharkmail.ocean','Thanks @Nibbles! [POST:makko.sandbar#2]','2025-01-01 11:50:00+00'),
  ('makko.sandbar@sharkmail.ocean','makko.sandbar@sharkmail.ocean','Quiet lagoon vibes. [POST:makko.sandbar#3]','2025-01-01 12:00:00+00'),

  -- Nibbles
  ('nibbles.nurse@sharkmail.ocean','nibbles.nurse@sharkmail.ocean','Surveying coral growth. [POST:nibbles.nurse#1]','2025-01-01 12:10:00+00'),
  ('nibbles.nurse@sharkmail.ocean','jet.lemon@sharkmail.ocean','Courier legend @Jet. [POST:nibbles.nurse#2]','2025-01-01 12:20:00+00'),
  ('nibbles.nurse@sharkmail.ocean','nibbles.nurse@sharkmail.ocean','Seahorse rescue done. [POST:nibbles.nurse#3]','2025-01-01 12:30:00+00'),

  -- Jet
  ('jet.lemon@sharkmail.ocean','jet.lemon@sharkmail.ocean','Harbour patrol clear. [POST:jet.lemon#1]','2025-01-01 12:40:00+00'),
  ('jet.lemon@sharkmail.ocean','finn.white@sharkmail.ocean','Drop-off for @Finn. [POST:jet.lemon#2]','2025-01-01 12:50:00+00'),
  ('jet.lemon@sharkmail.ocean','jet.lemon@sharkmail.ocean','It’s been a long week patrolling the outer reefs, and I’ve been thinking about how currents and code have more in common than most sharks realise.
When you dive deep enough, you start noticing patterns — the gentle oscillations that carry you forward if you learn to move with them, the turbulence that hits when you fight against the flow. It’s exactly like debugging a stubborn asynchronous task: once you stop forcing it, everything starts aligning naturally.
This morning, I watched a school of sardines reorganise itself after a barracuda attack — thousands of individuals moving as one, reacting in milliseconds. It reminded me of a great dev team shipping a big feature: everyone trusting the same rhythm, no panic, just clean direction changes.
I think that’s what I love about building Acebook with the SeaSharks crew. There’s something poetic about how our tiny functions and view models all fit together, how one misplaced semicolon can cause chaos just like a ripple through the reef.
By the time the sun hit the kelp tops, I’d written three more migration scripts in my head and came up with a new tagline: "Swim with intention, code with flow."
Anyway, if you made it this far, thanks for reading my ramble. Leave a like if you’ve ever had a eureka moment mid-swim or found beauty in a well-structured async method. 🌊💻
#OceanThoughts #SeaSharks #CSharpCurrents #Acebook [POST:jet.lemon#3]','2025-01-01 13:00:00+00')
)
INSERT INTO "Posts" ("UserId","WallId","Content","CreatedOn")
SELECT a."Id", w."Id", p.content, p.created_on::timestamptz
FROM p
JOIN "Users" a ON a."Email" = p.author
JOIN "Users" w ON w."Email" = p.wall;

-- ---------- COMMENTS (4 per user)
-- We target posts by the unique [POST:<email>#n] tag in content.
WITH cmt(c_email, post_tag, content, created_on) AS (
  VALUES
  -- Finn comments
  ('finn.white@sharkmail.ocean','[POST:shelly.tiger#1]','Fin-tastic!','2025-01-01 13:10:00+00'),
  ('finn.white@sharkmail.ocean','[POST:bruce.hammerhead#1]','See you by the ridge.','2025-01-01 13:11:00+00'),
  ('finn.white@sharkmail.ocean','[POST:jet.lemon#1]','Smooth patrol!','2025-01-01 13:12:00+00'),
  ('finn.white@sharkmail.ocean','[POST:finn.white#3]','Kelp latte on me.','2025-01-01 13:13:00+00'),

  -- Shelly
  ('shelly.tiger@sharkmail.ocean','[POST:finn.white#1]','Love the dawn swims.','2025-01-01 13:14:00+00'),
  ('shelly.tiger@sharkmail.ocean','[POST:coral.reef#1]','Nursery heroes!','2025-01-01 13:15:00+00'),
  ('shelly.tiger@sharkmail.ocean','[POST:bruce.hammerhead#3]','West tides are rough.','2025-01-01 13:16:00+00'),
  ('shelly.tiger@sharkmail.ocean','[POST:shelly.tiger#3]','Teamwork wins.','2025-01-01 13:17:00+00'),

  -- Bruce
  ('bruce.hammerhead@sharkmail.ocean','[POST:shelly.tiger#2]','Thanks for the shout!','2025-01-01 13:18:00+00'),
  ('bruce.hammerhead@sharkmail.ocean','[POST:reef.blue#1]','Heart soaring indeed.','2025-01-01 13:19:00+00'),
  ('bruce.hammerhead@sharkmail.ocean','[POST:coral.reef#3]','Ridge glide is zen.','2025-01-01 13:20:00+00'),
  ('bruce.hammerhead@sharkmail.ocean','[POST:bruce.hammerhead#1]','Canyon thoughts hit.','2025-01-01 13:21:00+00'),

  -- Coral
  ('coral.reef@sharkmail.ocean','[POST:finn.white#2]','Trench sprints 🔥','2025-01-01 13:22:00+00'),
  ('coral.reef@sharkmail.ocean','[POST:reef.blue#2]','Great patrol!','2025-01-01 13:23:00+00'),
  ('coral.reef@sharkmail.ocean','[POST:jet.lemon#3]','Stay safe out there.','2025-01-01 13:24:00+00'),
  ('coral.reef@sharkmail.ocean','[POST:coral.reef#1]','Thanks team!','2025-01-01 13:25:00+00'),

  -- Reef
  ('reef.blue@sharkmail.ocean','[POST:coral.reef#2]','Currents were wild!','2025-01-01 13:26:00+00'),
  ('reef.blue@sharkmail.ocean','[POST:shelly.tiger#1]','Dolphins are hilarious.','2025-01-01 13:27:00+00'),
  ('reef.blue@sharkmail.ocean','[POST:reef.blue#3]','Kelp latte gang.','2025-01-01 13:28:00+00'),
  ('reef.blue@sharkmail.ocean','[POST:nibbles.nurse#3]','Great rescue!','2025-01-01 13:29:00+00'),

  -- Tigra
  ('tigra.mako@sharkmail.ocean','[POST:reef.blue#1]','Nice tempo!','2025-01-01 13:30:00+00'),
  ('tigra.mako@sharkmail.ocean','[POST:basky.bull#2]','Ripple dance ftw.','2025-01-01 13:31:00+00'),
  ('tigra.mako@sharkmail.ocean','[POST:tigra.mako#3]','Blue calm.','2025-01-01 13:32:00+00'),
  ('tigra.mako@sharkmail.ocean','[POST:makko.sandbar#1]','Cobalt mornings 🤝','2025-01-01 13:33:00+00'),

  -- Basky
  ('basky.bull@sharkmail.ocean','[POST:tigra.mako#2]','Team up anytime.','2025-01-01 13:34:00+00'),
  ('basky.bull@sharkmail.ocean','[POST:makko.sandbar#2]','Let’s go.','2025-01-01 13:35:00+00'),
  ('basky.bull@sharkmail.ocean','[POST:basky.bull#3]','Shimmering indeed.','2025-01-01 13:36:00+00'),
  ('basky.bull@sharkmail.ocean','[POST:nibbles.nurse#1]','Survey looks solid.','2025-01-01 13:37:00+00'),

  -- Makko
  ('makko.sandbar@sharkmail.ocean','[POST:basky.bull#1]','Canyon quiet hits.','2025-01-01 13:38:00+00'),
  ('makko.sandbar@sharkmail.ocean','[POST:nibbles.nurse#2]','Courier king!','2025-01-01 13:39:00+00'),
  ('makko.sandbar@sharkmail.ocean','[POST:makko.sandbar#3]','Lagoon life.','2025-01-01 13:40:00+00'),
  ('makko.sandbar@sharkmail.ocean','[POST:jet.lemon#2]','Drop-offs on time.','2025-01-01 13:41:00+00'),

  -- Nibbles
  ('nibbles.nurse@sharkmail.ocean','[POST:jet.lemon#1]','Harbour clear ✅','2025-01-01 13:42:00+00'),
  ('nibbles.nurse@sharkmail.ocean','[POST:finn.white#1]','Dawn magic.','2025-01-01 13:43:00+00'),
  ('nibbles.nurse@sharkmail.ocean','[POST:nibbles.nurse#1]','Logbook updated.','2025-01-01 13:44:00+00'),
  ('nibbles.nurse@sharkmail.ocean','[POST:coral.reef#3]','Zen agreed.','2025-01-01 13:45:00+00'),

  -- Jet
  ('jet.lemon@sharkmail.ocean','[POST:finn.white#3]','Save me a latte.','2025-01-01 13:46:00+00'),
  ('jet.lemon@sharkmail.ocean','[POST:shelly.tiger#2]','Shout received!','2025-01-01 13:47:00+00'),
  ('jet.lemon@sharkmail.ocean','[POST:reef.blue#2]','Great patrol team.','2025-01-01 13:48:00+00'),
  ('jet.lemon@sharkmail.ocean','[POST:jet.lemon#3]','Currents brisk indeed.','2025-01-01 13:49:00+00')
)
INSERT INTO "Comments" ("UserId","PostId","Content","CreatedOn")
SELECT u."Id", p."Id", cmt.content, cmt.created_on::timestamptz
FROM cmt
JOIN "Users" u ON u."Email" = cmt.c_email
JOIN "Posts" p ON p."Content" LIKE '%' || cmt.post_tag || '%';

-- ---------- POST LIKES (4 per user)
-- Target specific post tags; choose a mix of self & others.
WITH pl(liker, post_tag) AS (
  VALUES
  -- Finn
  ('finn.white@sharkmail.ocean','[POST:finn.white#1]'),
  ('finn.white@sharkmail.ocean','[POST:shelly.tiger#1]'),
  ('finn.white@sharkmail.ocean','[POST:jet.lemon#1]'),
  ('finn.white@sharkmail.ocean','[POST:bruce.hammerhead#1]'),

  -- Shelly
  ('shelly.tiger@sharkmail.ocean','[POST:shelly.tiger#1]'),
  ('shelly.tiger@sharkmail.ocean','[POST:finn.white#2]'),
  ('shelly.tiger@sharkmail.ocean','[POST:coral.reef#1]'),
  ('shelly.tiger@sharkmail.ocean','[POST:bruce.hammerhead#3]'),

  -- Bruce
  ('bruce.hammerhead@sharkmail.ocean','[POST:bruce.hammerhead#1]'),
  ('bruce.hammerhead@sharkmail.ocean','[POST:shelly.tiger#2]'),
  ('bruce.hammerhead@sharkmail.ocean','[POST:reef.blue#1]'),
  ('bruce.hammerhead@sharkmail.ocean','[POST:coral.reef#3]'),

  -- Coral
  ('coral.reef@sharkmail.ocean','[POST:coral.reef#1]'),
  ('coral.reef@sharkmail.ocean','[POST:finn.white#1]'),
  ('coral.reef@sharkmail.ocean','[POST:reef.blue#2]'),
  ('coral.reef@sharkmail.ocean','[POST:jet.lemon#3]'),

  -- Reef
  ('reef.blue@sharkmail.ocean','[POST:reef.blue#3]'),
  ('reef.blue@sharkmail.ocean','[POST:coral.reef#2]'),
  ('reef.blue@sharkmail.ocean','[POST:shelly.tiger#1]'),
  ('reef.blue@sharkmail.ocean','[POST:nibbles.nurse#3]'),

  -- Tigra
  ('tigra.mako@sharkmail.ocean','[POST:tigra.mako#3]'),
  ('tigra.mako@sharkmail.ocean','[POST:reef.blue#1]'),
  ('tigra.mako@sharkmail.ocean','[POST:basky.bull#2]'),
  ('tigra.mako@sharkmail.ocean','[POST:makko.sandbar#1]'),

  -- Basky
  ('basky.bull@sharkmail.ocean','[POST:basky.bull#1]'),
  ('basky.bull@sharkmail.ocean','[POST:tigra.mako#2]'),
  ('basky.bull@sharkmail.ocean','[POST:makko.sandbar#2]'),
  ('basky.bull@sharkmail.ocean','[POST:nibbles.nurse#1]'),

  -- Makko
  ('makko.sandbar@sharkmail.ocean','[POST:makko.sandbar#3]'),
  ('makko.sandbar@sharkmail.ocean','[POST:basky.bull#1]'),
  ('makko.sandbar@sharkmail.ocean','[POST:nibbles.nurse#2]'),
  ('makko.sandbar@sharkmail.ocean','[POST:jet.lemon#2]'),

  -- Nibbles
  ('nibbles.nurse@sharkmail.ocean','[POST:nibbles.nurse#1]'),
  ('nibbles.nurse@sharkmail.ocean','[POST:jet.lemon#1]'),
  ('nibbles.nurse@sharkmail.ocean','[POST:finn.white#1]'),
  ('nibbles.nurse@sharkmail.ocean','[POST:coral.reef#3]'),

  -- Jet
  ('jet.lemon@sharkmail.ocean','[POST:jet.lemon#3]'),
  ('jet.lemon@sharkmail.ocean','[POST:finn.white#3]'),
  ('jet.lemon@sharkmail.ocean','[POST:shelly.tiger#2]'),
  ('jet.lemon@sharkmail.ocean','[POST:reef.blue#2]')
)
INSERT INTO "Likes" ("UserId","PostId","CommentId")
SELECT u."Id", p."Id", NULL
FROM pl
JOIN "Users" u ON u."Email" = pl.liker
JOIN "Posts" p ON p."Content" LIKE '%' || pl.post_tag || '%';

-- ---------- COMMENT LIKES (5 per user)
-- Target concrete comments via commenter + post tag + comment content hint.
-- (Keeps it deterministic and readable; PostId stays NULL.)
WITH cl(liker, target_commenter, post_tag, content_hint) AS (
  VALUES
  -- Finn
  ('finn.white@sharkmail.ocean','shelly.tiger@sharkmail.ocean','[POST:bruce.hammerhead#3]','West tides'),
  ('finn.white@sharkmail.ocean','jet.lemon@sharkmail.ocean','[POST:reef.blue#2]','Great patrol'),
  ('finn.white@sharkmail.ocean','coral.reef@sharkmail.ocean','[POST:reef.blue#2]','Great patrol'),
  ('finn.white@sharkmail.ocean','basky.bull@sharkmail.ocean','[POST:tigra.mako#2]','Team up'),
  ('finn.white@sharkmail.ocean','nibbles.nurse@sharkmail.ocean','[POST:jet.lemon#1]','Harbour'),

  -- Shelly
  ('shelly.tiger@sharkmail.ocean','finn.white@sharkmail.ocean','[POST:finn.white#3]','latte'),
  ('shelly.tiger@sharkmail.ocean','bruce.hammerhead@sharkmail.ocean','[POST:coral.reef#3]','Ridge glide'),
  ('shelly.tiger@sharkmail.ocean','coral.reef@sharkmail.ocean','[POST:coral.reef#1]','Thanks team'),
  ('shelly.tiger@sharkmail.ocean','reef.blue@sharkmail.ocean','[POST:nibbles.nurse#3]','Great rescue'),
  ('shelly.tiger@sharkmail.ocean','jet.lemon@sharkmail.ocean','[POST:shelly.tiger#2]','Shout received'),

  -- Bruce
  ('bruce.hammerhead@sharkmail.ocean','shelly.tiger@sharkmail.ocean','[POST:shelly.tiger#1]','Dolphins'),
  ('bruce.hammerhead@sharkmail.ocean','finn.white@sharkmail.ocean','[POST:jet.lemon#1]','Smooth patrol'),
  ('bruce.hammerhead@sharkmail.ocean','reef.blue@sharkmail.ocean','[POST:reef.blue#3]','latte'),
  ('bruce.hammerhead@sharkmail.ocean','jet.lemon@sharkmail.ocean','[POST:finn.white#3]','latte'),
  ('bruce.hammerhead@sharkmail.ocean','basky.bull@sharkmail.ocean','[POST:nibbles.nurse#1]','Survey'),

  -- Coral
  ('coral.reef@sharkmail.ocean','finn.white@sharkmail.ocean','[POST:jet.lemon#1]','Smooth patrol'),
  ('coral.reef@sharkmail.ocean','shelly.tiger@sharkmail.ocean','[POST:shelly.tiger#3]','Teamwork'),
  ('coral.reef@sharkmail.ocean','bruce.hammerhead@sharkmail.ocean','[POST:bruce.hammerhead#1]','Canyon'),
  ('coral.reef@sharkmail.ocean','jet.lemon@sharkmail.ocean','[POST:reef.blue#2]','patrol'),
  ('coral.reef@sharkmail.ocean','nibbles.nurse@sharkmail.ocean','[POST:finn.white#1]','Dawn'),

  -- Reef
  ('reef.blue@sharkmail.ocean','shelly.tiger@sharkmail.ocean','[POST:coral.reef#1]','Nursery'),
  ('reef.blue@sharkmail.ocean','coral.reef@sharkmail.ocean','[POST:finn.white#2]','Trench'),
  ('reef.blue@sharkmail.ocean','basky.bull@sharkmail.ocean','[POST:basky.bull#3]','Shimmering'),
  ('reef.blue@sharkmail.ocean','nibbles.nurse@sharkmail.ocean','[POST:coral.reef#3]','Zen'),
  ('reef.blue@sharkmail.ocean','jet.lemon@sharkmail.ocean','[POST:jet.lemon#3]','Currents'),

  -- Tigra
  ('tigra.mako@sharkmail.ocean','reef.blue@sharkmail.ocean','[POST:reef.blue#1]','tempo'),
  ('tigra.mako@sharkmail.ocean','basky.bull@sharkmail.ocean','[POST:tigra.mako#2]','Team up'),
  ('tigra.mako@sharkmail.ocean','makko.sandbar@sharkmail.ocean','[POST:makko.sandbar#3]','Lagoon'),
  ('tigra.mako@sharkmail.ocean','jet.lemon@sharkmail.ocean','[POST:finn.white#3]','latte'),
  ('tigra.mako@sharkmail.ocean','nibbles.nurse@sharkmail.ocean','[POST:nibbles.nurse#2]','Courier'),

  -- Basky
  ('basky.bull@sharkmail.ocean','tigra.mako@sharkmail.ocean','[POST:reef.blue#1]','Nice tempo'),
  ('basky.bull@sharkmail.ocean','makko.sandbar@sharkmail.ocean','[POST:jet.lemon#2]','Drop-offs'),
  ('basky.bull@sharkmail.ocean','coral.reef@sharkmail.ocean','[POST:coral.reef#2]','Currents'),
  ('basky.bull@sharkmail.ocean','jet.lemon@sharkmail.ocean','[POST:shelly.tiger#2]','Shout'),
  ('basky.bull@sharkmail.ocean','finn.white@sharkmail.ocean','[POST:finn.white#1]','dawn'),

  -- Makko
  ('makko.sandbar@sharkmail.ocean','basky.bull@sharkmail.ocean','[POST:tigra.mako#2]','Team up'),
  ('makko.sandbar@sharkmail.ocean','nibbles.nurse@sharkmail.ocean','[POST:nibbles.nurse#1]','Logbook/Survey'),
  ('makko.sandbar@sharkmail.ocean','reef.blue@sharkmail.ocean','[POST:shelly.tiger#1]','Dolphins'),
  ('makko.sandbar@sharkmail.ocean','jet.lemon@sharkmail.ocean','[POST:jet.lemon#1]','Harbour'),
  ('makko.sandbar@sharkmail.ocean','coral.reef@sharkmail.ocean','[POST:coral.reef#3]','Zen'),

  -- Nibbles
  ('nibbles.nurse@sharkmail.ocean','jet.lemon@sharkmail.ocean','[POST:jet.lemon#3]','Currents brisk'),
  ('nibbles.nurse@sharkmail.ocean','finn.white@sharkmail.ocean','[POST:finn.white#3]','latte'),
  ('nibbles.nurse@sharkmail.ocean','shelly.tiger@sharkmail.ocean','[POST:shelly.tiger#1]','Dolphins'),
  ('nibbles.nurse@sharkmail.ocean','basky.bull@sharkmail.ocean','[POST:basky.bull#1]','Canyon'),
  ('nibbles.nurse@sharkmail.ocean','reef.blue@sharkmail.ocean','[POST:reef.blue#3]','latte'),

  -- Jet
  ('jet.lemon@sharkmail.ocean','finn.white@sharkmail.ocean','[POST:finn.white#1]','reef at dawn'),
  ('jet.lemon@sharkmail.ocean','shelly.tiger@sharkmail.ocean','[POST:shelly.tiger#3]','Teamwork'),
  ('jet.lemon@sharkmail.ocean','bruce.hammerhead@sharkmail.ocean','[POST:bruce.hammerhead#1]','Canyon thoughts'),
  ('jet.lemon@sharkmail.ocean','makko.sandbar@sharkmail.ocean','[POST:makko.sandbar#1]','Cobalt'),
  ('jet.lemon@sharkmail.ocean','nibbles.nurse@sharkmail.ocean','[POST:nibbles.nurse#3]','rescue')
)
INSERT INTO "Likes" ("UserId","PostId","CommentId")
SELECT lu."Id", NULL, c."Id"
FROM cl
JOIN "Users" lu ON lu."Email" = cl.liker
JOIN "Users" cu ON cu."Email" = cl.target_commenter
JOIN "Posts" p ON p."Content" LIKE '%' || cl.post_tag || '%'
JOIN "Comments" c
  ON c."UserId" = cu."Id"
 AND c."PostId" = p."Id"
 AND c."Content" ILIKE '%' || cl.content_hint || '%';


-- UPDATE "Posts" SET "Content" = regexp_replace("Content", '\[POST:[^]]+\]\s*', '', 'gi');
-- psql acebook_csharp_development < ../dev_seed.sql 