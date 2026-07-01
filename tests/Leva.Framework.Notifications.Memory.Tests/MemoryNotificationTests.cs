using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Leva.Framework.Notifications;
using Leva.Framework.Notifications.Memory;
using Xunit;

namespace Leva.Framework.Notifications.Memory.Tests;

public sealed class MemoryNotificationTests
{
	[Fact]
	public async Task SendAsync_RecordsSentNotification()
	{
		var gateway = new MemoryNotificationGateway();
		var notification = TestNotification();

		ResultAssert.Success(await gateway.SendAsync(notification));
		Assert.Single(gateway.Sent);
		Assert.Equal(notification, gateway.Sent[0]);
	}

	[Fact]
	public async Task Store_SaveLoadAndClear_WorkInMemory()
	{
		var store = new MemoryNotificationStore();
		var notification = TestNotification();
		var entry = NotificationEntry.Sent(notification, DateTimeOffset.UtcNow);

		ResultAssert.Success(await store.SaveAsync(entry));
		var loadedEntry = ResultAssert.Success(await store.LoadAsync(notification.Id));
		Assert.Equal(entry, loadedEntry);

		Assert.Single(ResultAssert.Success(await store.LoadAllAsync()));
		ResultAssert.Success(await store.DeleteAsync(notification.Id));
		Assert.Empty(ResultAssert.Success(await store.LoadAllAsync()));

		ResultAssert.Success(await store.SaveAsync(entry));
		store.Clear();
		Assert.Empty(store.Entries);
	}

	[Fact]
	public async Task MemoryNotificationServices_CreatesWorkingServiceGroup()
	{
		var services = MemoryNotificationServices.Create();
		var entry = ResultAssert.Success(
			await services.NotificationService.SendAsync(
				new NotificationRecipient("principal-1"),
				new NotificationChannel("in-app"),
				"Subject",
				"Body"
			)
		);

		Assert.Equal(NotificationStatus.Sent, entry.Status);
		Assert.Single(services.Gateway.Sent);
		Assert.Single(services.Store.Entries);
	}

	private static Notification TestNotification()
	{
		return new(
			NotificationId.New(),
			new NotificationRecipient("principal-1", "user@example.com", "User One"),
			new NotificationChannel("email"),
			"Subject",
			"Body",
			DateTimeOffset.UtcNow
		);
	}
}
