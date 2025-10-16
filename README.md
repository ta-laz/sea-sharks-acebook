# AceBook

## Quickstart

First, clone this repository. Then:

- Install the .NET Entity Framework CLI
  * `dotnet tool install --global dotnet-ef`
- Create the database/s in `psql`
  * `CREATE DATABASE acebook_csharp_development;`
  * `CREATE DATABASE acebook_csharp_test;`
- Run the migration to create the tables
  * `cd` into `/Acebook`
  * `dotnet ef database update`
  * `DATABASE_NAME=acebook_csharp_development dotnet ef database update`
- Start the application, with the development database
  * `DATABASE_NAME=acebook_csharp_development dotnet watch run`
- Go to `http://localhost:5287/`

## Running the Tests

- Install playwright package
  * `dotnet add package Microsoft.Playwright`
- Install playwright browser
  * `pwsh bin/Debug/net9.0/playwright.ps1 install`
- Verify its installation
 * `dotnet list package`
- Start the application, with the default (test) database
  * `dotnet watch run`
- Open a second terminal session and run the tests
  * `dotnet test`

### Troubleshooting

If you see a popup about not being able to open Chromedriver...
- Go to **System Preferences > Security and Privacy > General**
- There should be another message about Chromedriver there
- If so, Click on **Allow Anyway**

## Updating the Database

Changes are applied to the database programatically, using files called _migrations_, which live in the `/Migrations` directory. The process is as follows...

- To update an existing table
  * For example, you might want to add a title to the `Post` model
  * In which case, you would add a new field there
- To create a new table
  * For example, you might want to add a table called Comments
  * First, create the `Comment` model
  * Then go to AcebookDbContext
  * And add this `public DbSet<Comment>? Comments { get; set; }` 
- Generate the migration file
  * `cd` into `/Acebook`
  * Decide what you wan to call the migration file
  * `AddTitleToPosts` or `CreateCommentsTable` would be good descriptive names
  * Then do `dotnet ef migrations add ` followed by the name you chose
  * E.g.  `dotnet ef migrations add AddTitleToPosts`
- Run the migration
  * `dotnet ef database update`

### Troubleshooting

#### Seeing `role "postgres" does not exist`?

Your application tries to connect to the database as a user called `postgres`, which is normally created automatically when you install PostgresQL. If the `postgres` user doesn't exist, you'll see `role "postgres" does not exist`.

To fix it, you'll need to create the `postgres` user.

Try this in your terminal...

```
; createuser -s postgres
```

If you see `command not found: createuser`, start a new `psql` session and do this...

```sql
create user postgres;
```

#### Want to Change an Existing Migration?

Don't edit the migration files after they've been applied / run. If you do that, it'll probably lead to problems. If you decide that the migration you just applied wasn't quite right for some reason, you have two options

- Create and run another migration (using the process above)

OR...

- Rollback / undo the last migration
- Then edit the migration file before re-running it

How do you rollbacl a migration? Let's assume that you have two migrations, both of which have been applied.

1. CreatePostsAndUsers
2. AddTitleToPosts

To rollback the second, you again use `dotnet ef database update` but this time adding the name of the last 'good' migration. In this case, that would be `CreatePostsAndUsers`. So the command is...

```shell
; dotnet ef database update CreatePostsAndUsers
```
##### Project OverVeiw:

This app allows one to sign themselves up to a aquatic ecosystem where they can make post their thoughts into the tank where they are subjected to being liked and commented on by other finned swimmers. You can add these fun fishies as your friends and swim together in the Aquarium and even post on eachothers walls or unfriend them if your swimming on a different direction. This application exists purely as a successful effort of Sea Sharks team effort, brilliant planning and love for tests!

-Tech Stack:

ASP.NET Core MVC (.NET 9)

Entity Framework Core

Razor Views (HTML + Tailwind CSS)

SQLite / PostgreSQL / SQL Server (whichever DB you used)

xUnit / NUnit / Playwright (for testing)

-Project Structure: Quick list of major folders and their purpose:

/Controllers – request handling logic

/Models – database and data transfer objects

/Views – Razor pages for rendering UI

/wwwroot – static files (CSS, images)

/Data – database context, migrations


##### Architecture & Design:
2.1 Overview

SeaShark follows the Model–View–Controller (MVC) architectural pattern using ASP.NET Core MVC.
This separation of concerns improves maintainability, testability, and scalability of the application.

Model – Represents the data and business logic. Entity Framework Core models such as User, Post, and Comment define the database schema and relationships.

View – Responsible for displaying information to the user through Razor .cshtml templates, styled with Tailwind CSS and Flowbite components.

Controller – Handles incoming HTTP requests, interacts with the Model, and selects which View to render.

All requests pass through controllers, which fetch or manipulate data using the AcebookDbContext before returning an appropriate view.

2.2 MVC Request Flow
[Browser] → [Controller] → [Model / DbContext] → [Controller] → [View] → [Browser]


Example:

A user visits /search?SearchString=shark.

The request is routed to SearchBarController.Index().

The controller queries the database via AcebookDbContext, filtering users, posts, and comments.

The results are packaged into ViewBag objects.

Razor view /Views/SearchBar/Index.cshtml renders the HTML.

The browser displays the filtered results.

2.3 Application Layers
Layer	Purpose	Example Classes / Files
Model	Defines entities and relationships. Manages business logic and validation.	User.cs, Post.cs, Comment.cs, AcebookDbContext.cs
View	Presents data to users via Razor pages with Tailwind CSS for styling.	_Layout.cshtml, Views/Users/Index.cshtml, Views/SearchBar/Index.cshtml
Controller	Handles HTTP requests and responses, invoking models and selecting views.	UsersController.cs, PostsController.cs, SearchBarController.cs
Database Context	Provides a bridge between models and the underlying database using EF Core.	AcebookDbContext.cs
2.4 Design Decisions

Entity Framework Core:
Chosen for its integration with ASP.NET Core, allowing easy database migrations and LINQ-based querying.

Razor Views with TailwindCSS:
Offers a fast, utility-first design workflow that keeps styles consistent and compact across the app.

Session-based Authentication:
Used instead of JWT for simplicity; session stores user_id and user_name for persistent sign-in.

Search Filtering System:
A single SearchBarController manages queries for Users, Posts, and Comments.
The controller accepts a SearchString and an optional SearchFilter parameter to determine which entities to query.

###### Database Design: 

3.1 Overview

SeaShark uses Entity Framework Core (EF Core) as its Object–Relational Mapper (ORM) to manage all database interactions.
The data model follows a relational design where users, posts, comments, likes, and friendships are interconnected through primary–foreign key relationships.

The database context is defined in AcebookDbContext.cs, which exposes DbSet properties for each entity, enabling LINQ-based querying and automatic schema generation through EF migrations.

3.2 Entity Summary
Entity	Description
User	Represents a registered account in the system. Each user can create posts, comments, likes, and manage friendships.
Post	Represents a user’s post on their own or another user’s wall. Can have many comments and likes.
Comment	Represents user replies to posts. Comments can also be liked.
Like	Represents a like action on a post or comment. Each like is tied to a user and either a post or comment.
Friend	Manages relationships between two users, supporting friend requests and accepted connections.
ProfileBio	Stores extended profile information for a user, such as biography and profile picture path.
3.3 Relationships Between Entities
Relationship	Type	Description
User → Post	One-to-Many	A user can author multiple posts (User.Posts).
User → Comment	One-to-Many	A user can write multiple comments (User.Comments).
User → Like	One-to-Many	A user can like many posts or comments (User.Likes).
Post → Comment	One-to-Many	A post can have multiple comments (Post.Comments).
Post → Like	One-to-Many	A post can have multiple likes (Post.Likes).
Comment → Like	One-to-Many	A comment can be liked by many users.
User ↔ Friend	Many-to-Many (self-referencing)	A user can have many friends through the Friend entity.
User → ProfileBio	One-to-One	Each user has one corresponding profile bio.
3.4 Entity Diagrams
User:
User
│ Id : int (PK)
│ FirstName : string
│ LastName : string
│ Email : string
│ Password : string
│ DOB : DateTime
│ CreatedOn : DateTime
│
├── ICollection<Post> Posts
├── ICollection<Comment> Comments
├── ICollection<Like> Likes
├── ICollection<Friend> Friends
└── ProfileBio ProfileBio

Post:
Post
│ Id : int (PK)
│ Content : string
│ UserId : int (FK → User)
│ WallId : int
│ CreatedOn : DateTime
│ PostPicturePath : string?
│
├── ICollection<Comment> Comments
└── ICollection<Like> Likes

Comment:
Comment
│ Id : int (PK)
│ Content : string
│ UserId : int (FK → User)
│ PostId : int (FK → Post)
│ CreatedOn : DateTime
│
└── ICollection<Like> Likes

Like:
Like
│ Id : int (PK)
│ UserId : int (FK → User)
│ PostId : int? (FK → Post)
│ CommentId : int? (FK → Comment)
│
├── Post? Post
├── Comment? Comment
└── User User

Friend:
Friend
│ Id : int (PK)
│ SenderId : int (FK → User)
│ ReceiverId : int (FK → User)
│ Status : string (e.g., "Pending", "Accepted")
│ CreatedAt : DateTime

ProfileBio:
ProfileBio
│ Id : int (PK)
│ UserId : int (FK → User)
│ Biography : string
│ ProfilePicturePath : string?
│ CoverPicturePath : string?

3.5 Entity Relationships Diagram (ERD)
erDiagram
    User ||--o{ Post : "creates"
    User ||--o{ Comment : "writes"
    User ||--o{ Like : "reacts"
    User ||--o{ Friend : "connects"
    User ||--|| ProfileBio : "has"

    Post ||--o{ Comment : "receives"
    Post ||--o{ Like : "receives"
    Comment ||--o{ Like : "receives"

    Friend }o--|| User : "receiver"

#### Controllers:

#### Core Features:

##### Frontend/ Views:
6.1 Overview

SeaShark’s front-end presentation layer is built using Razor Views (.cshtml) combined with Tailwind CSS and Flowbite for modern, responsive styling.
The application adopts a modular view structure, where shared layout files define common UI components (navigation bar, footer, scripts), and page-specific Razor views render dynamic content.

Each view interacts with its respective controller to display data from the database through ViewData, ViewBag, or strongly typed models (@model).

6.2 Layout & Shared Components
File	Purpose
_Layout.cshtml	The global layout template wrapping all pages. Contains navigation bar, search bar, user dropdown, and footer. Defines consistent structure across the site.
_Layout.cshtml.css	Custom stylesheet providing styling for navigation, footer, and layout spacing. Works alongside Tailwind for additional layout control

_Layout.cshtml


_ValidationScriptsPartial.cshtml	Partial view used for validation scripts across user forms (e.g. registration, login, profile update).
Error.cshtml	Displays application errors or invalid requests gracefully, showing a message with debugging information when enabled.
Key Features

Dynamic Navbar: Shows different links and buttons depending on whether a user is signed in (based on Session variables).

Integrated Search Bar: The top navigation includes an interactive search system with category filtering (All, People, Posts, Comments).

Responsive Design: Tailwind’s grid and utility classes ensure consistent styling across mobile, tablet, and desktop screens.

Reusable Footer: Shared footer with privacy and policy links included in _Layout.cshtml.

6.3 Page Views
Views/Home/Index.cshtml

Serves as the landing page of SeaShark.

Provides quick access to posts or user feeds after login.

Pulls data from HomeController to populate the feed.

Views/Users/MyProfile.cshtml

Displays the currently logged-in user’s profile information.

Shows profile bio (ProfileBio model), post history, and edit options.

Integrates Update.cshtml partial for in-page profile updates.

Accesses data via UsersController.Index() or similar routes.

Views/Users/OtherProfile.cshtml

Used to display another user’s public profile when visited from a search or post interaction.

Fetches posts authored by the selected user and friendship status (via Friend model).

Views/Posts/Post.cshtml

Displays a single post in detail view, including all comments and likes.

Uses Razor conditionals to check if the post exceeds a certain character length and formats accordingly (@post.CheckLength() and @post.FormatPostContent() methods).

Contains interactive “Like” and “Comment” buttons styled with Flowbite and Tailwind hover effects.

Views/Comments/Index.cshtml

Shows all comments for a specific post.

Renders user names, timestamps, and like counts dynamically.

Uses partial post rendering for consistency.

Views/Users/ViewUserFriends.cshtml

Displays the user’s friends list and pending friend requests.

Loops through Friend entities, showing both Sender and Receiver names and statuses.

Implements conditional logic to differentiate between accepted, pending, or requested friendships.

Views/Users/New.cshtml

Renders the user registration page with data validation (integrated _ValidationScriptsPartial).

Collects details like first name, last name, email, and password.

Communicates with UsersController.New() for validation and database insertion.

Views/Users/Update.cshtml

Form for updating user or profile bio details.

Accepts image uploads and text updates (Biography, Profile Picture).

Posts form data back to the controller for persistence.

🔍 Views/SearchBar/Index.cshtml

Displays search results for users, posts, and comments.

Uses ViewBag.UsersResults, ViewBag.PostsResults, and ViewBag.CommentsResults to render relevant sections conditionally.

Integrates seamlessly with the navbar search form via query parameters SearchString and SearchFilter.

🔒 Views/Home/Privacy.cshtml

Displays privacy policy and app terms.

Linked from the footer in _Layout.cshtml.

6.4 Visual Flow
graph LR
A[_Layout.cshtml] --> B[Navigation / Search Bar]
A --> C[Main Body via @RenderBody()]
C --> D[Dynamic Views e.g. MyProfile, Post, SearchBar]
D --> E[Controllers]
E --> F[Models & DbContext]


This structure ensures all pages inherit the same layout, and each feature-specific view communicates with its corresponding controller to retrieve and render data.

6.5 Frontend Technologies
Technology	Usage
Razor (.cshtml)	Combines C# and HTML for dynamic page rendering.
Tailwind CSS	Provides utility-first responsive design classes.
Flowbite	Adds pre-styled components (buttons, dropdowns, modals).
JavaScript (minimal)	Handles dropdown interaction and search filter logic.
Razor Layout System	Ensures consistent navbar, footer, and theme across all views.

#### Testing:

7.1 Overview

Testing within SeaShark (Acebook) ensures the reliability, consistency, and performance of both front-end and back-end features.
The testing suite combines Playwright (for browser-based end-to-end tests) with NUnit (for unit and integration tests), ensuring full coverage of user flows such as authentication, posting, commenting, and friend interactions.

7.2 Testing Frameworks
Framework	Description
NUnit	Core test framework for unit and integration testing.
Playwright for .NET	Provides headless browser automation to simulate real user interactions (login, posting, navigation).
TestDataSeeder	Seeds the database before each test to maintain consistency and reproducibility.
AcebookDbContext	Used across tests to interact with a clean database instance for each test cycle.
7.3 Browser-Based End-to-End Tests
FriendsListPage.Tests.cs

Verifies that the friends list displays correctly for logged-in users.

Confirms filtering between My Friends, Received Requests, and Sent Requests.

Tests friend request acceptance, search functionality, and redirection to a friend’s profile.

Example:

await Page.ClickAsync("#received-label");
await Expect(Page.GetByText("My Received Requests")).ToBeVisibleAsync();

OtherProfilePage.Tests.cs

Ensures that when viewing another user’s profile:

Taglines display correctly under the user’s name.

“Write on Wall” input behaves correctly depending on friendship status.

The Add Friend, Unfriend, and Friend Request Sent buttons update dynamically.

“See All Friends” and “View Profile” buttons navigate correctly.

Covers wall posts, comment button interactions, and redirect logic:

await Page.GetByTestId("add-friend").ClickAsync();
await Expect(Page.GetByTestId("friend-request-sent")).ToBeVisibleAsync();

PhotoUpload.Tests.cs

Tests photo upload functionality from the user’s profile or post creation area.

Validates that uploaded photos are stored and rendered correctly in the UI.

Confirms error handling for unsupported file types and sizes.

Simulates file uploads using Playwright’s file chooser API:

var fileChooser = await Page.RunAndWaitForFileChooserAsync(() => 
    Page.ClickAsync("#upload-button"));
await fileChooser.SetFilesAsync("test_image.jpg");

7.4 Unit and Integration Tests
Test File	Coverage
UserModel.Test.cs	Tests model logic such as FormattedCreatedOn date formatting and CheckLength() helpers.
UserManagement.Test.cs	Ensures user creation, login, and validation work as expected.
PostPage.Test.cs	Verifies posting, commenting, and like functionality across views.
SearchResults.Tests.cs	Tests filtering logic for users, posts, and comments.
NavBar.Test.cs	Confirms visibility and functionality of navigation bar items depending on authentication state.
LandingPage.Tests.cs	Verifies homepage rendering and route redirection for new and returning users.
MyProfilePage.Tests.cs	Ensures personal profile information updates and visibility of user posts.

#### Future Implementations:

-
-
-
-
-
-
-

#### Acknowledgements:
Below is a list of all external documentation sources, frameworks, and technologies referenced or used throughout the SeaShark (Acebook) project.
Each entry includes a short explanation of its purpose and how it directly supports your implementation.

- ASP.NET Core MVC Framework

Docs: ASP.NET Core MVC Overview https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-9.0

Purpose: Provides the Model-View-Controller architecture that powers SeaShark’s routing, controllers, and views.
Relevance:

Used for all controllers such as SearchBarController, PostsController, and UsersController.

Enables ViewData, ViewBag, and IActionResult usage for rendering Razor views.

- Entity Framework Core (EF Core)

Docs: Entity Framework Core Documentation https://learn.microsoft.com/en-us/ef/core/

Purpose: Object-relational mapper (ORM) handling all database queries and persistence.
Relevance:

Used in AcebookDbContext for defining DbSet<User>, DbSet<Post>, DbSet<Comment>, and DbSet<Friend>.

Enables LINQ queries such as .Include(), .ThenInclude(), and .Where() across your models.

- Razor View Engine

Docs: Razor Syntax Reference https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-9.0

Purpose: Defines the templating language for .cshtml views.
Relevance:

Used throughout your front-end (_Layout.cshtml, Index.cshtml, MyProfile.cshtml, OtherProfile.cshtml).

Enables @model, @foreach, and conditionals to bind data to HTML.

-Tailwind CSS 

Docs: Tailwind CSS Documentation https://tailwindcss.com/docs/installation/using-vite

Purpose: Utility-first CSS framework used for responsive design and consistent styling.
Relevance:

Used across all views for component styling (bg-teal-600, rounded-lg, shadow-sm).

Simplifies consistent design across user pages and the main navigation bar.

-Flowbite

Docs: Flowbite Components https://flowbite.com/docs/getting-started/introduction/

Purpose: Tailwind-based component library used for dropdowns, modals, and UI elements.
Relevance:

Implements the interactive “All Categories” dropdown and search bar functionality.

Enhances navigation and dynamic user menus in _Layout.cshtml.

-ASP.NET Core Identity & Session

Docs: ASP.NET Core Identity https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0&tabs=visual-studio

Purpose: Handles authentication, user sessions, and authorization logic.
Relevance:

Used in session management (Context.Session.GetInt32("user_id"), GetString("user_name")).

Enables login persistence and secure logout (Signout in SessionsController).

-Dependency Injection (DI)

Docs: ASP.NET Core Dependency Injection https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-9.0

Purpose: Provides loose coupling and automatic service resolution in controllers.
Relevance:

Injects dependencies like ILogger<SearchBarController> and AcebookDbContext into controllers.

Improves testability and separation of concerns.

-Testing Frameworks
-NUnit

Docs: NUnit Documentation https://docs.nunit.org/articles/nunit/intro.html

Purpose: Framework for unit testing and integration testing backend logic.
Relevance:

Used in tests such as UserModel.Test.cs, UserManagement.Test.cs, and SearchResults.Tests.cs.

-Playwright for .NET

Docs: Playwright .NET Docs https://playwright.dev/dotnet/docs/intro

Purpose: Enables automated browser-based testing for UI validation.
Relevance:

Used in FriendsListPage.Tests.cs, OtherProfilePage.Tests.cs, and PhotoUpload.Tests.cs to simulate real user interactions.

-.NET CLI

Docs: .NET CLI Documentation https://learn.microsoft.com/en-us/dotnet/core/tools/

Purpose: Provides command-line tools for building, testing, and managing .NET projects.
Relevance:

Used for commands like dotnet run, dotnet test, and dotnet ef database update.

Enables running Playwright tests and installing browsers via PowerShell.

- Language & Query Tools 
LINQ (Language-Integrated Query)

Docs: LINQ Overview https://learn.microsoft.com/en-us/dotnet/csharp/linq/

Purpose: Provides concise query syntax to filter and project data within EF Core.
Relevance:

Used in queries such as:

users.Where(u => 
    u.Posts.Any(p => p.Content.Contains(SearchString)) ||
    u.FirstName.Contains(SearchString) ||
    u.LastName.Contains(SearchString)
);

-C# Language Reference

Docs: C# Reference (Microsoft) https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/

Purpose: Authoritative reference for all C# syntax and features.
Relevance:

Applies to custom logic, data models, and controller flow (e.g., async/await, lambda expressions).

-Routing & HTTP Controllers 

Docs: Controller Action Methods in ASP.NET Core https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions?view=aspnetcore-9.0

Purpose: Explains controller patterns, attribute routing, and HTTP verbs.
Relevance:

Used throughout SeaShark’s controllers (e.g., [Route("/Search")], [HttpGet], [HttpPost]).

Guides how the search filter and user interactions are handled in the backend.


-NuGet Package Manager 

Docs: NuGet Documentation https://learn.microsoft.com/en-us/nuget/what-is-nuget

Purpose: Manages external packages and libraries for .NET projects.
Relevance:

Used for installing Microsoft.Playwright, NUnit, and EntityFrameworkCore dependencies.

Enables updating and maintaining consistent library versions across environments.

-Razor Layouts & Partials

Docs: Razor Layouts in ASP.NET Core https://learn.microsoft.com/en-us/aspnet/core/mvc/views/layout?view=aspnetcore-9.0

Purpose: Manages shared HTML structure and layout consistency across views.
Relevance:

Implemented in _Layout.cshtml to unify navigation, search, and footer across all pages.

🧩 Summary of Key Documentation Areas
Category	Documentation	Use Case
Core Framework	ASP.NET Core MVC
	Controllers, routing, views
Database	Entity Framework Core
	Data access & relationships
Frontend	Tailwind CSS
 + Flowbite
	Styling & components
Authentication	ASP.NET Identity
	Session & user auth
Testing	NUnit
 + Playwright .NET
	Backend & E2E tests
Deployment	.NET CLI
 + GitHub Actions
	Builds, automation & CI
Language Tools	LINQ
 + C# Reference
	Queries & syntax

#### What did this project teach us?
to work together 
cant trust sarah
sarah can do a trello really well
tom can do exacildraw pefectly

