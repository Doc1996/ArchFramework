using Leva.Framework.Fakes;
using Leva.Framework.Notifications;
using Leva.Framework.Notifications.Memory;
using Xunit;

namespace Leva.Framework.Contract.Tests;

public sealed class NotificationStoreContractTests
{
	[Theory]
	[InlineData("memory")]
	[InlineData("fake")]
	public async Task NotificationStoreContract_SaveLoadReplaceLoadAllAndDelete_AreConsistent(string storeName)
	{
		var store = CreateStore(storeName);
		var first = SentEntry("subject-1");
		var second = SentEntry("subject-2");

		var failedReplacement = NotificationEntry.Failed(
			first.Notification,
			first.CompletedAt.AddMinutes(1),
			NotificationErrors.Failed("send", "replacement")
		);

		ResultAssert.Success(await store.SaveAsync(first));
		ResultAssert.Success(await store.SaveAsync(second));
		Assert.Equal(first, ResultAssert.Success(await store.LoadAsync(first.NotificationId)));

		ResultAssert.Success(await store.SaveAsync(failedReplacement));
		Assert.Equal(failedReplacement, ResultAssert.Success(await store.LoadAsync(first.NotificationId)));
		var all = ResultAssert.Success(await store.LoadAllAsync());

		Assert.Equal(2, all.Count);
		Assert.Contains(failedReplacement, all);
		Assert.Contains(second, all);

		ResultAssert.Success(await store.DeleteAsync(first.NotificationId));
		Assert.Null(ResultAssert.Success(await store.LoadAsync(first.NotificationId)));
		Assert.Equal(second, Assert.Single(ResultAssert.Success(await store.LoadAllAsync())));
	}

	[Theory]
	[InlineData("memory")]
	[InlineData("fake")]
	public async Task NotificationStoreContract_DeleteMissingEntryIsIdempotent(string storeName)
	{
		var store = CreateStore(storeName);
		var id = NotificationId.New();

		ResultAssert.Success(await store.DeleteAsync(id));
		Assert.Null(ResultAssert.Success(await store.LoadAsync(id)));
	}

	private static INotificationStore CreateStore(string storeName) =>
		storeName switch
		{
			"memory" => new MemoryNotificationStore(),
			"fake" => new FakeNotificationStore(),
			_ => throw new ArgumentOutOfRangeException(nameof(storeName), storeName, "Unknown notification store."),
		};

	private static NotificationEntry SentEntry(string subject)
	{
		var now = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
		var notification = new Notification(
			NotificationId.New(),
			new NotificationRecipient("principal-1", "user@example.com", "User One"),
			new NotificationChannel("in-app"),
			subject,
			"Body",
			now
		);

		return NotificationEntry.Sent(notification, now.AddSeconds(1));
	}
}
