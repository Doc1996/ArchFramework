using Leva.Framework.Core;
using Leva.Framework.Notifications;
using Leva.Framework.Testing;
using Xunit;

namespace Leva.Framework.Fakes.Tests;

public sealed class FakeNotificationSenderTests
{
	[Fact]
	public async Task SendAsync_CanFailNextSendWithoutRecordingNotification()
	{
		var sender = new FakeNotificationSender();
		var error = NotificationErrors.Failed("send", "Configured failure.");

		sender.FailNext(error);
		var result = await sender.SendAsync(TestNotification());

		Assert.True(result.IsFailure);
		Assert.Equal(error, result.Error);
		Assert.Empty(sender.Sent);
	}

	private static Notification TestNotification()
	{
		return new(
			NotificationId.New(),
			new NotificationRecipient("principal-1"),
			NotificationChannel.InApp,
			"Subject",
			"Body",
			DateTimeOffset.UtcNow
		);
	}
}
