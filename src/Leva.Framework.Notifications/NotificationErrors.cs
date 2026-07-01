using Leva.Framework.Core;

namespace Leva.Framework.Notifications;

/// <summary>
/// Creates common structured errors for notification gateways and stores.
/// </summary>
public static class NotificationErrors
{
	public static Error Failed(string operation, string? details = null) =>
		new("notification.failed", WithDetails($"Notification operation '{operation}' failed.", details));

	public static Error Unavailable(string gateway, string? details = null) =>
		new("notification.unavailable", WithDetails($"Notification gateway '{gateway}' is unavailable.", details));

	public static Error UnsupportedChannel(NotificationChannel channel) =>
		new("notification.unsupported_channel", $"Notification channel '{channel}' is not supported.");

	private static string WithDetails(string message, string? details) =>
		string.IsNullOrWhiteSpace(details) ? message : $"{message} {details}";
}
