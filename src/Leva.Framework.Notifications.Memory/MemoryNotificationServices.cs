using Leva.Framework.Core;
using Leva.Framework.Notifications;

namespace Leva.Framework.Notifications.Memory;

/// <summary>
/// Groups common in-memory notification services for development and tests.
/// </summary>
public sealed class MemoryNotificationServices(
	MemoryNotificationGateway gateway,
	MemoryNotificationStore store,
	NotificationService notificationService
)
{
	public MemoryNotificationGateway Gateway { get; } = gateway;
	public MemoryNotificationStore Store { get; } = store;
	public NotificationService NotificationService { get; } = notificationService;

	public static MemoryNotificationServices Create(IClock? clock = null)
	{
		var usedClock = clock ?? new SystemClock();
		var gateway = new MemoryNotificationGateway();
		var store = new MemoryNotificationStore();

		return new MemoryNotificationServices(gateway, store, new NotificationService(gateway, store, usedClock));
	}
}
