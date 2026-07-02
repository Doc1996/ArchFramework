using Microsoft.AspNetCore.SignalR;

namespace Leva.Framework.Notifications.SignalR;

/// <summary>
/// SignalR hub used to deliver notifications to connected clients.
/// </summary>
public sealed class SignalRNotificationHub : Hub<ISignalRNotificationClient>;
