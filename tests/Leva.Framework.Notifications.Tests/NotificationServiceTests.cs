using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Leva.Framework.Notifications;
using Xunit;

namespace Leva.Framework.Notifications.Tests;

public sealed class NotificationServiceTests
{
	[Fact]
	public async Task SendAsync_CreatesNotificationAndStoresEntry()
	{
		var clock = new FakeClock(new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero));
		var gateway = new FakeNotificationGateway();
		var store = new FakeNotificationStore();

		var service = new NotificationService(gateway, store, clock);
		var entry = ResultAssert.Success(
			await service.SendAsync(
				new NotificationRecipient("principal-1", "user@example.com", "User One"),
				new NotificationChannel("email"),
				"Subject",
				"Body"
			)
		);

		Assert.Equal(NotificationStatus.Sent, entry.Status);
		Assert.Single(gateway.Sent);
		Assert.Single(store.Entries);

		Assert.Equal("Subject", gateway.Sent[0].Subject);
		Assert.Equal("Body", gateway.Sent[0].Body);
	}

	[Fact]
	public async Task SendAsync_StoresFailedEntry()
	{
		var clock = new FakeClock();
		var gateway = new FakeNotificationGateway();
		var store = new FakeNotificationStore();

		var service = new NotificationService(gateway, store, clock);
		gateway.FailNext(NotificationErrors.Failed("send", "Configured failure."));

		var result = await service.SendAsync(
			new NotificationRecipient("principal-1"),
			new NotificationChannel("in-app"),
			"Subject",
			"Body"
		);

		Assert.True(result.IsFailure);
		Assert.Empty(gateway.Sent);
		var failed = Assert.Single(store.Entries);
		Assert.Equal(NotificationStatus.Failed, failed.Status);
	}

	[Fact]
	public async Task SendAsync_StoresCancelledEntry()
	{
		var clock = new FakeClock();
		var gateway = new FakeNotificationGateway();
		var store = new FakeNotificationStore();

		var service = new NotificationService(gateway, store, clock);
		gateway.CancelNext();

		var result = await service.SendAsync(
			new NotificationRecipient("principal-1"),
			new NotificationChannel("in-app"),
			"Subject",
			"Body"
		);

		Assert.True(result.IsFailure);
		Assert.Empty(gateway.Sent);
		var cancelled = Assert.Single(store.Entries);
		Assert.Equal(NotificationStatus.Cancelled, cancelled.Status);
	}

	[Fact]
	public async Task SendAsync_ReturnsFailureWhenStoreFails()
	{
		var clock = new FakeClock();
		var gateway = new FakeNotificationGateway();
		var store = new FakeNotificationStore();

		var service = new NotificationService(gateway, store, clock);
		var error = NotificationErrors.Failed("save", "Configured failure.");
		store.FailNextSave(error);

		var result = await service.SendAsync(
			new NotificationRecipient("principal-1"),
			new NotificationChannel("in-app"),
			"Subject",
			"Body"
		);

		Assert.True(result.IsFailure);
		Assert.Equal(error, result.Error);
		Assert.Single(gateway.Sent);
		Assert.Empty(store.Entries);
	}
}
