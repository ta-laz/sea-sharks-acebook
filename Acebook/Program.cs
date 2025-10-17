using acebook.Hubs;
using acebook.Models;
using Microsoft.AspNetCore.Identity;

var configBuilder = new ConfigurationBuilder();
configBuilder.AddEnvironmentVariables();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Adding the whole SignalR thing here which allowsd for real-time notifications
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ MUST come after UseRouting and before authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// set up a connection to the hub (needed by SignalR I believe to set up a clear like session for the user?)
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
