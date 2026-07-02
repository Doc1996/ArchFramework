using Leva.Framework.Notifications;

namespace Leva.Framework.Sample.LiveDashboard;

internal sealed record DashboardNotificationEntry(
	string Status,
	string Subject,
	string Body,
	string Recipient,
	DateTimeOffset CreatedAt,
	DateTimeOffset CompletedAt
)
{
	public static DashboardNotificationEntry From(NotificationEntry entry) =>
		new(
			entry.Status.ToString(),
			entry.Notification.Subject,
			entry.Notification.Body,
			entry.Notification.Recipient.DisplayName ?? entry.Notification.Recipient.Id,
			entry.CreatedAt,
			entry.CompletedAt
		);
}
