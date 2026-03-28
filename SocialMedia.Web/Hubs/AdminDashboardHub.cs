using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SocialMedia.Web.Hubs
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardHub: Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            await base.OnConnectedAsync();
        }

  
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
