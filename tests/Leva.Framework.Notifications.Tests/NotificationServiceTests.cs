using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Leva.Framework.Notifications;
using Leva.Framework.Testing;
using Xunit;

namespace Leva.Framework.Notifications.Tests;

public sealed class NotificationServiceTests
{
	[Fact]
	public async Task SendAsync_CreatesNotificationAndStoresEntry()
	{
		var clock = new FakeClock(new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero));
		var sender = new FakeNotificationSender();
		var store = new FakeNotificationStore();

		var service = new NotificationService(sender, store, clock);
		var entry = ResultAssert.Success(
			await service.SendAsync(
				new NotificationRecipient("principal-1", "user@example.com", "User One"),
				NotificationChannel.Email,
				"Subject",
				"Body"
			)
		);

		Assert.Equal(NotificationStatus.Sent, entry.Status);
		Assert.Single(sender.Sent);
		Assert.Single(store.Entries);

		Assert.Equal("Subject", sender.Sent[0].Subject);
		Assert.Equal("Body", sender.Sent[0].Body);
	}

	[Fact]
	public async Task SendAsync_StoresFailedEntry()
	{
		var clock = new FakeClock();
		var sender = new FakeNotificationSender();
		var store = new FakeNotificationStore();

		var service = new NotificationService(sender, store, clock);
		sender.FailNext(NotificationErrors.Failed("send", "Configured failure."));

		var result = await service.SendAsync(
			new NotificationRecipient("principal-1"),
			NotificationChannel.InApp,
			"Subject",
			"Body"
		);

		Assert.True(result.IsFailure);
		Assert.Empty(sender.Sent);
		var failed = Assert.Single(store.Entries);
		Assert.Equal(NotificationStatus.Failed, failed.Status);
	}

	[Fact]
	public async Task SendAsync_ReturnsFailureWhenStoreFails()
	{
		var clock = new FakeClock();
		var sender = new FakeNotificationSender();
		var store = new FakeNotificationStore();

		var service = new NotificationService(sender, store, clock);
		var error = NotificationErrors.Failed("save", "Configured failure.");
		store.FailNextSave(error);

		var result = await service.SendAsync(
			new NotificationRecipient("principal-1"),
			NotificationChannel.InApp,
			"Subject",
			"Body"
		);

		Assert.True(result.IsFailure);
		Assert.Equal(error, result.Error);
		Assert.Single(sender.Sent);
		Assert.Empty(store.Entries);
	}
}
