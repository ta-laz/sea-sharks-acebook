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
# 🦈 SeaShark (Acebook)

---

## 📘 Project Overview

This app allows users to sign themselves up to an aquatic ecosystem where they can post their thoughts into the tank — where they are subject to being liked and commented on by other finned swimmers. 🐠  

You can add these fun fishies as your friends and swim together in the Aquarium — even post on each other’s walls or unfriend them if you’re swimming in a different direction.  

This application exists purely as a successful effort of the **Sea Sharks** team: teamwork, brilliant planning, and a love for tests! ❤️  

---

## ⚙️ Tech Stack

- **ASP.NET Core MVC (.NET 9)**
- **Entity Framework Core**
- **Razor Views (HTML + Tailwind CSS)**
- **SQLite / PostgreSQL / SQL Server** (depending on environment)
- **xUnit / NUnit / Playwright** (for testing)

---

## 📁 Project Structure

| Folder | Description |
|--------|--------------|
| `/Controllers` | Request handling logic |
| `/Models` | Database entities and data transfer objects |
| `/Views` | Razor pages for rendering UI |
| `/wwwroot` | Static files (CSS, JS, images) |
| `/Data` | Database context and EF migrations |

---

## 🧩 Architecture & Design

### 2.1 Overview
SeaShark follows the **Model–View–Controller (MVC)** architectural pattern using ASP.NET Core MVC.  
This separation of concerns improves **maintainability**, **testability**, and **scalability**.

- **Model** – Represents data and business logic via EF Core models (e.g., `User`, `Post`, `Comment`).
- **View** – Displays information through Razor `.cshtml` templates styled with Tailwind CSS & Flowbite.
- **Controller** – Handles HTTP requests, interacts with models, and renders appropriate views.

All requests pass through controllers, which fetch or manipulate data using `AcebookDbContext` before returning a view.

### 2.2 MVC Request Flow
```
[Browser] → [Controller] → [Model / DbContext] → [Controller] → [View] → [Browser]
```

**Example:**
- User visits `/search?SearchString=shark`
- Routed to `SearchBarController.Index()`
- Controller queries via `AcebookDbContext` filtering users, posts, and comments
- Results passed through `ViewBag`
- Razor view `/Views/SearchBar/Index.cshtml` renders HTML
- Browser displays filtered results

### 2.3 Application Layers

| Layer | Purpose | Example Files |
|-------|----------|---------------|
| **Model** | Defines entities, validation, and logic | `User.cs`, `Post.cs`, `Comment.cs`, `AcebookDbContext.cs` |
| **View** | Presents data using Razor and Tailwind | `_Layout.cshtml`, `Views/SearchBar/Index.cshtml` |
| **Controller** | Handles HTTP requests and responses | `UsersController.cs`, `PostsController.cs`, `SearchBarController.cs` |
| **Database Context** | Bridge between EF Core models and DB | `AcebookDbContext.cs` |

### 2.4 Design Decisions
- **Entity Framework Core:** Simplifies ORM, migrations, and LINQ querying  
- **Razor + TailwindCSS:** Lightweight, utility-first design system  
- **Session-based Authentication:** Simple and secure for prototype scale  
- **Search Filtering System:** One controller (`SearchBarController`) queries Users, Posts, and Comments using `SearchFilter`

---

## 🐬 Database Design

### 3.1 Overview
SeaShark uses **Entity Framework Core (EF Core)** for ORM and relational database management.  
`AcebookDbContext.cs` defines all tables (DbSets), enabling LINQ queries and schema generation.

### 3.2 Entity Summary

| Entity | Description |
|--------|--------------|
| **User** | Represents a registered account. Can create posts, comments, likes, and friendships. |
| **Post** | Represents a user’s post on their wall or another’s. Supports comments and likes. |
| **Comment** | Represents replies to posts. Can be liked. |
| **Like** | Represents user reactions to posts/comments. |
| **Friend** | Manages friend relationships and requests. |
| **ProfileBio** | Stores biography, profile image, and cover photo. |

### 3.3 Relationships

| Relationship | Type | Description |
|--------------|------|-------------|
| User → Post | One-to-Many | A user authors many posts |
| User → Comment | One-to-Many | A user can make many comments |
| User → Like | One-to-Many | A user can like many posts/comments |
| Post → Comment | One-to-Many | A post can have many comments |
| Post → Like | One-to-Many | A post can have many likes |
| Comment → Like | One-to-Many | A comment can have many likes |
| User ↔ Friend | Many-to-Many | Friendship managed through `Friend` |
| User → ProfileBio | One-to-One | Each user has one profile bio |

### 3.4 Entity Relationship Diagram
```mermaid
erDiagram
  USER ||--o{ POST : creates
  USER ||--o{ COMMENT : writes
  USER ||--o{ LIKE : reacts

  %% Friend as a join entity with two roles
  FRIEND }o--|| USER : requester
  FRIEND }o--|| USER : accepter


diagram for notifications:

  %% Notification with sender/receiver roles
  NOTIFICATION }o--|| USER : receiver
  NOTIFICATION }o--|| USER : sender

  POST ||--o{ COMMENT : receives
  POST ||--o{ LIKE : receives
  COMMENT ||--o{ LIKE : receives
```

---

## Core Features

- 🧍‍♂️ User authentication and sessions  
- 🗣️ Posting and commenting system  
- 💙 Likes on posts and comments  
- 🧑‍🤝‍🧑 Friend requests and mutual friendships  
- 🔍 Search bar with category filters (People / Posts / Comments)  
- 📸 Profile bio, images, and cover photo management  

---

## 🖥️ Frontend / Views

### 4.1 Overview
Built with **Razor Views**, **Tailwind CSS**, and **Flowbite**, the frontend is fully responsive and consistent.

**Shared Layouts:**
- `_Layout.cshtml`: Global layout with navigation, search bar, and footer  
- `_Layout.cshtml.css`: Custom layout styling  
- `_ValidationScriptsPartial.cshtml`: Validation for forms  
- `Error.cshtml`: Friendly error page  

### 4.2 Page Views

| View | Description |
|------|--------------|
| **Home/Index.cshtml** | Landing page displaying post feed |
| **Users/MyProfile.cshtml** | Displays user’s own profile and posts |
| **Users/OtherProfile.cshtml** | Public profile of another user |
| **Posts/Post.cshtml** | Detailed post view with comments & likes |
| **Comments/Index.cshtml** | Displays comments under a post |
| **Users/ViewUserFriends.cshtml** | Shows friend list and requests |
| **Users/New.cshtml** | User registration form |
| **Users/Update.cshtml** | Edit user bio or upload photos |
| **SearchBar/Index.cshtml** | Renders search results by category |
| **Home/Privacy.cshtml** | App privacy policy |

### 4.4 Frontend Technologies

| Tech | Purpose |
|------|----------|
| Razor (.cshtml) | Dynamic C# + HTML templating |
| Tailwind CSS | Utility-first responsive design |
| Flowbite | Prebuilt Tailwind components |
| JavaScript | Dropdowns, filters, and search logic |
| Razor Layout System | Shared navigation, theme, and footer |

---
## 🎮 Controllers

### 5.1 Overview
SeaShark’s controllers act as the bridge between user interactions and backend logic.  
Each controller manages a specific part of the system — from user authentication and friendships to posting, commenting, and liking.  
Controllers follow the MVC pattern, ensuring a clean separation between **data (Models)**, **logic (Controllers)**, and **presentation (Views)**.

### 5.2 Controller Summary

| Controller | Description |
|-------------|--------------|
| **HomeController** | Handles default routes such as `/`, `/Privacy`, and `/Error`. Manages static pages and global view rendering. |
| **UsersController** | Manages user sign-up (`/signup`), profile display (`/users/{id}`), and profile updates. Handles profile picture uploads and bio edits. Uses session-based authentication. |
| **SessionsController** | Handles login (`/signin`) and logout (`/signout`) logic. Hashes passwords using SHA256 and manages session variables like `user_id` and `user_profile_picture`. |
| **PostsController** | Handles creation, deletion, and retrieval of posts. Supports wall posting on self or friends’ profiles, includes like and comment data via eager loading (`Include`, `ThenInclude`). |
| **CommentsController** | Manages comment creation and association with posts. Handles comment deletion and related like relationships. |
| **LikesController** | Manages user “like” actions on posts and comments. Toggles like states, preventing duplicate entries and maintaining relational integrity. |
| **FriendsController** | Manages friend requests, acceptances, and removals. Implements logic to show friend lists, pending requests, and sent requests, filtered per logged-in user. |
| **SearchBarController** | Handles the `/Search` route. Accepts `SearchString` and `SearchFilter` parameters to query Users, Posts, and Comments dynamically. Returns results via `ViewBag` and Razor rendering. |
| **NotificationsController** | Manages the `/notifications` route, handling real-time and stored alerts for users. Retrieves, sends, and marks notifications as `read` using `AcebookDbContext` and SignalR for live updates. Returns data through Razor views or JSON responses. |

### 5.3 Common Design Patterns

- **Session-Based User Context:**  
  Controllers rely on session variables (`user_id`, `user_name`, `user_profile_picture`) to identify the current user securely.

- **Model Binding & Validation:**  
  Uses `[HttpPost]` and `[ValidateAntiForgeryToken]` attributes to validate form submissions and prevent CSRF attacks.

- **Service Filters:**  
  `AuthenticationFilter` ensures protected routes like `/users/{id}` and `/users/{id}/update` can only be accessed by authenticated users.

- **LINQ with EF Core:**  
  Each controller uses `Include()`, `ThenInclude()`, and `Where()` to efficiently query relational data.

- **Redirect Flow:**  
  Successful actions (login, signup, upload) return `RedirectResult` or `RedirectToAction` to refresh data-driven views (e.g., `/posts`, `/users/{id}`).

### 5.4 Example Request Flow

**Scenario:** A user logs in and visits their friend’s profile.  

1. **Login:**  
   - `POST /signin` → `SessionsController.Create()` validates credentials and stores session data.  
2. **Profile Access:**  
   - `GET /users/{id}` → `UsersController.Index()` loads the requested user with posts, comments, and friendship data.  
3. **Friendship Check:**  
   - The controller queries `Friends` table for relationship status between current user and profile owner.  
4. **View Rendered:**  
   - Depending on context, Razor renders either `MyProfile.cshtml` or `OtherProfile.cshtml` with contextual ViewBag data.  

### 5.5 Security & Validation

- ✅ **Password Hashing:** SHA256 implemented before storing user credentials.  
- ✅ **CSRF Protection:** `[ValidateAntiForgeryToken]` used across all POST routes.  
- ✅ **Authorization Checks:** Routes protected via session checks and `AuthenticationFilter`.  
- ✅ **Error Handling:** `ErrorViewModel` and `Error()` methods standardised for global exception handling.  

---

## 🧪 Testing

### 6.1 Overview
Testing ensures feature reliability using **NUnit** and **Playwright for .NET**.

### 6.2 Testing Frameworks

| Framework | Purpose |
|------------|----------|
| **NUnit** | Unit & integration testing |
| **Playwright** | End-to-end browser testing |
| **TestDataSeeder** | Seeds DB for consistent test data |

### 6.3 Example Tests

| File | Description |
|------|--------------|
| `FriendsListPage.Tests.cs` | Tests friend list rendering & filtering |
| `OtherProfilePage.Tests.cs` | Validates friend button logic & wall posts |
| `PhotoUpload.Tests.cs` | Simulates and validates file uploads |
| `UserModel.Test.cs` | Tests helper logic and date formatting |
| `SearchResults.Tests.cs` | Verifies search filtering by scope |
| `NavBar.Test.cs` | Checks navbar behaviour when logged in/out |
| `LandingPage.Tests.cs` | Ensures proper routing and page load |
| `MyProfilePage.Tests.cs` | Validates profile updates and data loading |

---

## 🚀 Future Implementations

- 📨 **Real-time notifications** for likes and comments  
- 💬 **Direct messaging** between users  
- 🧭 **Improved friend discovery** and suggestions  
- 🐚 **Media gallery** for post images and videos  
- 🔒 **Two-factor authentication**

---

## 🙏 Acknowledgements & Documentation

| Technology | Documentation | Purpose |
|-------------|----------------|----------|
| ASP.NET Core MVC | [ASP.NET Core MVC Overview](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-9.0) | Framework for controllers & routing |
| Entity Framework Core | [EF Core Docs](https://learn.microsoft.com/en-us/ef/core/) | ORM for DB access |
| Razor Views | [Razor Syntax Reference](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-9.0) | Template system |
| Tailwind CSS | [Tailwind Docs](https://tailwindcss.com/docs/installation/using-vite) | CSS framework |
| Flowbite | [Flowbite Docs](https://flowbite.com/docs/getting-started/introduction/) | UI components |
| ASP.NET Identity & Session | [ASP.NET Identity Docs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0) | Authentication system |
| Dependency Injection | [DI Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-9.0) | Service injection pattern |
| NUnit | [NUnit Docs](https://docs.nunit.org/articles/nunit/intro.html) | Testing framework |
| Playwright for .NET | [Playwright Docs](https://playwright.dev/dotnet/docs/intro) | End-to-end testing |
| .NET CLI | [.NET CLI Docs](https://learn.microsoft.com/en-us/dotnet/core/tools/) | Build & run tools |
| LINQ | [LINQ Overview](https://learn.microsoft.com/en-us/dotnet/csharp/linq/) | Data querying |
| C# Language | [C# Reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/) | Language reference |
| Routing & Controllers | [Controller Actions Docs](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions?view=aspnetcore-9.0) | Routing logic |
| NuGet | [NuGet Docs](https://learn.microsoft.com/en-us/nuget/what-is-nuget) | Package management |
| Razor Layouts | [Razor Layouts Docs](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/layout?view=aspnetcore-9.0) | Shared layouts |

---

## 🧭 Summary of Documentation

| Category | Documentation | Use Case |
|-----------|----------------|----------|
| Core Framework | ASP.NET Core MVC | Controllers, routing, views |
| Database | Entity Framework Core | Data access & relationships |
| Frontend | Tailwind + Flowbite | Styling & components |
| Authentication | ASP.NET Identity | Session & user auth |
| Testing | NUnit + Playwright | Backend & E2E tests |
| Deployment | .NET CLI | Build & automation |
| Language | LINQ, C# Reference | Queries & syntax |

---

🐚 **SeaShark — Dive deep, connect freely, and make a splash in your digital ocean.**
