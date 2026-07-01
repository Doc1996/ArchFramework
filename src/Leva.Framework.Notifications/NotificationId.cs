namespace Leva.Framework.Notifications;

/// <summary>
/// Stable identifier for a notification.
/// </summary>
public readonly record struct NotificationId(Guid Value)
{
	public static NotificationId New() => new(Guid.NewGuid());
}
