using Microsoft.AspNetCore.SignalR;

namespace TaskManagement.Services
{
    public class NotificationHub : Hub
    {
        // İleride özel bildirim metodları eklenebilir
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
} 