using Leva.Framework.Core;

namespace Leva.Framework.Notifications;

/// <summary>
/// Represents the stored status entry for a notification send operation.
/// </summary>
public sealed record NotificationEntry(
	Notification Notification,
	NotificationStatus Status,
	DateTimeOffset CompletedAt,
	Error? Error = null
)
{
	public NotificationId NotificationId => Notification.Id;
	public DateTimeOffset CreatedAt => Notification.CreatedAt;

	public static NotificationEntry Sent(Notification notification, DateTimeOffset completedAt) =>
		new(notification, NotificationStatus.Sent, completedAt);

	public static NotificationEntry Failed(Notification notification, DateTimeOffset completedAt, Error error) =>
		new(notification, NotificationStatus.Failed, completedAt, error);

	public static NotificationEntry Cancelled(
		Notification notification,
		DateTimeOffset completedAt,
		Error? error = null
	) => new(notification, NotificationStatus.Cancelled, completedAt, error);
}
