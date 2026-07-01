using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Leva.Framework.Notifications;
using Xunit;

namespace Leva.Framework.Fakes.Tests;

public sealed class FakeNotificationGatewayTests
{
	[Fact]
	public async Task SendAsync_CanFailNextSendWithoutRecordingNotification()
	{
		var gateway = new FakeNotificationGateway();
		var error = NotificationErrors.Failed("send", "Configured failure.");

		gateway.FailNext(error);
		var result = await gateway.SendAsync(TestNotification());

		Assert.True(result.IsFailure);
		Assert.Equal(error, result.Error);
		Assert.Empty(gateway.Sent);
	}

	[Fact]
	public async Task SendAsync_CanCancelNextSendWithoutRecordingNotification()
	{
		var gateway = new FakeNotificationGateway();
		gateway.CancelNext();

		await Assert.ThrowsAsync<OperationCanceledException>(() => gateway.SendAsync(TestNotification()));
		Assert.Empty(gateway.Sent);
	}

	private static Notification TestNotification()
	{
		return new(
			NotificationId.New(),
			new NotificationRecipient("principal-1"),
			new NotificationChannel("in-app"),
			"Subject",
			"Body",
			DateTimeOffset.UtcNow
		);
	}
}
