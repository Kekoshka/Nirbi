using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.WebApi.Common.Hubs
{
    /// <summary>
    /// Хаб для работы SignalR
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
