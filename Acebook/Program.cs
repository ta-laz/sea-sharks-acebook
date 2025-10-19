using System;
using System.IO;
using acebook.Hubs;
using acebook.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

//
//  Program.cs
//
try
{
    // ---- Optional .env loading (handy in Development) ----
    var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");

    if (File.Exists(envPath))
    {
        DotNetEnv.Env.Load(envPath); // load once
        Console.WriteLine($"🔧 Loaded .env from: {envPath}");

        // For Development you may want .env to override shell env
        if (envName.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var line in File.ReadAllLines(envPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                }
            }
            Console.WriteLine("🔁 Re-applied .env variables (overwrite mode for Development).");
        }
    }
    else
    {
        Console.WriteLine("ℹ️  No .env file found in current directory.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  .env load skipped: {ex.Message}");
}

var builder = WebApplication.CreateBuilder(args);

// ---- Helper: build per-environment connection string ----
static string BuildConnectionString(IHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        // Local dev/test via discrete env vars (or sensible defaults)
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var name = Environment.GetEnvironmentVariable("DB_NAME") ?? "acebook_csharp_development";
        var user = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var pass = Environment.GetEnvironmentVariable("DB_PASS") ?? "postgres";
        return $"Host={host};Database={name};Username={user};Password={pass}";
    }

    // Production on Render: DATABASE_URL e.g. postgres://user:pass@host:port/dbname?sslmode=require
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? throw new InvalidOperationException("DATABASE_URL not set");
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);

    // Parse optional query parameters (sslmode, etc.)
    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

    var csb = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : "",
        Database = uri.LocalPath.TrimStart('/'),
        Pooling = true
    };

    // Respect ?sslmode= if present; otherwise enforce secure defaults
    if (query["sslmode"] is string sslFromQuery)
    {
        csb.SslMode = Enum.TryParse<SslMode>(sslFromQuery, true, out var mode) ? mode : SslMode.Require;
    }
    else
    {
        csb.SslMode = SslMode.Require;
    }

    // In Render this is fine; you can flip to Prefer if you ever hit cert issues on internal URL
    csb.TrustServerCertificate = true;

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

// ✅ Register DbContext via DI
builder.Services.AddDbContext<AcebookDbContext>(options =>
{
    Console.WriteLine($"ENV ASPNETCORE_ENVIRONMENT = {builder.Environment.EnvironmentName}");
    Console.WriteLine($"ENV DB_NAME = {Environment.GetEnvironmentVariable("DB_NAME") ?? "(null)"}");

    var cs = BuildConnectionString(builder.Environment);

    // Avoid printing full secrets in Production logs
    if (builder.Environment.IsDevelopment())
        Console.WriteLine($"🌐 Using DB connection: {cs}");
    else
        Console.WriteLine("🌐 Using DB connection (details masked in Production).");

    options.UseNpgsql(cs, npg =>
    {
        if (!builder.Environment.IsDevelopment())
            npg.EnableRetryOnFailure();
    });
});

var app = builder.Build();

// ---- Proxy & HTTPS pipeline ----
// Forwarded headers (safe to always enable; often gated to non-Dev)
if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();     // Use Render’s TLS termination + redirect
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Static, routing, session, auth
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

// ---- Apply migrations on startup ----
// Default: apply in Production; opt-in locally via APPLY_MIGRATIONS_ON_STARTUP=true
var applyMigrationsEnv = Environment.GetEnvironmentVariable("APPLY_MIGRATIONS_ON_STARTUP");
bool applyMigrations =
    string.Equals(applyMigrationsEnv, "true", StringComparison.OrdinalIgnoreCase)
    || (!app.Environment.IsDevelopment() && !string.Equals(applyMigrationsEnv, "false", StringComparison.OrdinalIgnoreCase));

if (applyMigrations)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AcebookDbContext>();
    Console.WriteLine("🛠  Applying EF Core migrations on startup...");
    db.Database.Migrate();
    Console.WriteLine("✅ Migrations applied.");
}
else
{
    Console.WriteLine("⏭  Skipping migrations on startup (controlled by APPLY_MIGRATIONS_ON_STARTUP).");
}

// ---- Bind to Render port if present; otherwise keep launchSettings ports ----
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(renderPort))
{
    app.Urls.Add($"http://0.0.0.0:{renderPort}");
    Console.WriteLine($"🌐 Bound to Render PORT={renderPort}");
}
else
{
    Console.WriteLine("🌐 No PORT env var found; using launchSettings.json/local defaults.");
}

app.Run();
