using Microsoft.AspNetCore.SignalR;

namespace PrismERP.Api.Hubs;

/// <summary>
/// Real-time notifications hub. Clients join to receive ERP event pushes.
/// </summary>
public sealed class NotificationHub : Hub
{
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }
}
