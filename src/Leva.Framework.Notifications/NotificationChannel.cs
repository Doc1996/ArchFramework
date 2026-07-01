namespace Leva.Framework.Notifications;

/// <summary>
/// Identifies the delivery channel used by a notification.
/// </summary>
public readonly record struct NotificationChannel(string Value)
{
	public static NotificationChannel InApp { get; } = new("in-app");
	public static NotificationChannel Email { get; } = new("email");
	public static NotificationChannel Sms { get; } = new("sms");
	public static NotificationChannel Push { get; } = new("push");
}
