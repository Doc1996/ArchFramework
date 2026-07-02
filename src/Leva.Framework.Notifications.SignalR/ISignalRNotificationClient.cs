namespace Leva.Framework.Notifications.SignalR;

/// <summary>
/// Strongly typed SignalR client contract for receiving notification payloads.
/// </summary>
public interface ISignalRNotificationClient
{
	Task ReceiveNotification(SignalRNotificationPayload notification);
}
