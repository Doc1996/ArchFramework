using Leva.Framework.Notifications;

namespace Leva.Framework.Notifications.SignalR;

/// <summary>
/// Message contract sent from the SignalR notification hub to connected clients.
/// </summary>
public sealed record SignalRNotificationPayload(
	Guid Id,
	string RecipientId,
	string Channel,
	string Subject,
	string Body,
	DateTimeOffset CreatedAt
)
{
	public static SignalRNotificationPayload From(Notification notification)
	{
		ArgumentNullException.ThrowIfNull(notification);
		return new SignalRNotificationPayload(
			notification.Id.Value,
			notification.Recipient.Id,
			notification.Channel.Value,
			notification.Subject,
			notification.Body,
			notification.CreatedAt
		);
	}
}
