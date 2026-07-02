using Leva.Framework.Notifications;

namespace Leva.Framework.Notifications.SignalR;

/// <summary>
/// Configures SignalR notification delivery.
/// </summary>
public sealed class SignalRNotificationOptions
{
	public static NotificationChannel DefaultChannel { get; } = new("signalr");

	public NotificationChannel Channel { get; set; } = DefaultChannel;
}
