using acebook.Hubs;
using acebook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

// Optional: load .env only in Development (so teammates can keep local secrets out of git)
try
{
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
    {
        DotNetEnv.Env.Load(); // dotnet add package DotNetEnv
    }
}
catch { /* safe to ignore */ }

var builder = WebApplication.CreateBuilder(args);

// ---- Build a per-environment connection string ----
static string BuildConnectionString(IHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        // Local dev from environment variables (e.g. via .env)
        var host = Environment.GetEnvironmentVariable("DB_HOST")     ?? "localhost";
        var name = Environment.GetEnvironmentVariable("DB_NAME")     ?? "acebook_dev";
        var user = Environment.GetEnvironmentVariable("DB_USER")     ?? "postgres";
        var pass = Environment.GetEnvironmentVariable("DB_PASS")     ?? "postgres";

        return $"Host={host};Database={name};Username={user};Password={pass}";
    }

    // Production on Render: DATABASE_URL looks like postgres://user:pass@host:port/dbname
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? throw new InvalidOperationException("DATABASE_URL not set");
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);

    var csb = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : "",
        Database = uri.LocalPath.TrimStart('/'),
        // Render Postgres requires SSL
        SslMode = SslMode.Require,
        TrustServerCertificate = true,
        Pooling = true
    };
    return csb.ToString();
}

// ---- Services ----
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(600);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<acebook.ActionFilters.AuthenticationFilter>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// ✅ Register DbContext via DI (no hard-coded string)
builder.Services.AddDbContext<AcebookDbContext>(options =>
{
    var cs = BuildConnectionString(builder.Environment);
    options.UseNpgsql(cs, npg => npg.EnableRetryOnFailure());
});

var app = builder.Build();

// ---- Pipeline ----
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();  // ✅ enforce HTTPS only in prod
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Order matters
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

// ✅ (Optional but handy) Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AcebookDbContext>();
    db.Database.Migrate();
}

app.Run();