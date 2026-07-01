using Leva.Framework.Core;
using Leva.Framework.Notifications;

namespace Leva.Framework.Notifications.Memory;

/// <summary>
/// Groups common in-memory notification services for development and tests.
/// </summary>
public sealed class MemoryNotificationServices(
	MemoryNotificationSender sender,
	MemoryNotificationStore store,
	NotificationService notificationService
)
{
	public MemoryNotificationSender Sender { get; } = sender;
	public MemoryNotificationStore Store { get; } = store;
	public NotificationService NotificationService { get; } = notificationService;

	public static MemoryNotificationServices Create(IClock? clock = null)
	{
		var usedClock = clock ?? new SystemClock();
		var sender = new MemoryNotificationSender();
		var store = new MemoryNotificationStore();

		return new MemoryNotificationServices(sender, store, new NotificationService(sender, store, usedClock));
	}
}
