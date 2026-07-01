namespace Leva.Framework.Notifications;

/// <summary>
/// Represents a notification created for one recipient and one channel.
/// </summary>
public sealed record Notification(
	NotificationId Id,
	NotificationRecipient Recipient,
	NotificationChannel Channel,
	string Subject,
	string Body,
	DateTimeOffset CreatedAt
);
