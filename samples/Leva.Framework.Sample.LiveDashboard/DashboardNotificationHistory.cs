using Leva.Framework.Notifications;
using Leva.Framework.Notifications.Memory;

namespace Leva.Framework.Sample.LiveDashboard;

internal sealed class DashboardNotificationHistory(MemoryNotificationStore store)
{
	public IReadOnlyList<NotificationEntry> Entries => store.Entries;

	public async Task RecordAsync(NotificationEntry entry, CancellationToken token)
	{
		// Keep the visible dashboard history tied to the same concrete memory store.
		// Save is idempotent because MemoryNotificationStore is keyed by NotificationId.
		await store.SaveAsync(entry, token);
	}

	public DashboardNotificationEntry[] Snapshot() =>
		store.Entries.OrderByDescending(entry => entry.CompletedAt).Select(DashboardNotificationEntry.From).ToArray();
}
