using Microsoft.AspNetCore.SignalR;

namespace acebook.Hubs
{
    // The hub is what manages live connections between the server and each browser tab you as a usert have open
    public class NotificationHub : Hub
    {
        // When the user connects, it calls this method to join its own "group" so we can target notifications rather than broadcast to everyone
        public async Task RegisterUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
    }
}
