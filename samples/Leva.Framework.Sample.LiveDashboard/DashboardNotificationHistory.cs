using Leva.Framework.Notifications;
using Leva.Framework.Notifications.Memory;

namespace Leva.Framework.Sample.LiveDashboard;

internal sealed class DashboardNotificationHistory(MemoryNotificationStore store)
{
	public IReadOnlyList<NotificationEntry> Entries => store.Entries;

	public async Task RecordAsync(NotificationEntry entry, CancellationToken token)
	{
		// NotificationService normally stores entries through INotificationStore. The sample records the returned
		// entry into the concrete MemoryNotificationStore as well so the browser history always reads the same source.
		// MemoryNotificationStore is keyed by NotificationId, so this save is idempotent if the service already stored it.
		await store.SaveAsync(entry, token);
	}

	public DashboardNotificationEntry[] Snapshot() =>
		store.Entries.OrderByDescending(entry => entry.CompletedAt).Select(DashboardNotificationEntry.From).ToArray();
}
