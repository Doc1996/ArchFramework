using Leva.Framework.Fakes;
using Leva.Framework.Notifications;
using Leva.Framework.Notifications.Memory;
using Xunit;

namespace Leva.Framework.Contract.Tests;

public sealed class NotificationGatewayContractTests
{
	[Theory]
	[InlineData("memory")]
	[InlineData("fake")]
	public async Task NotificationGatewayContract_SendPreservesNotificationPayload(string gatewayName)
	{
		var scope = CreateGateway(gatewayName);
		var notification = TestNotification();

		ResultAssert.Success(await scope.Gateway.SendAsync(notification));
		Assert.Equal(notification, Assert.Single(scope.Sent()));
	}

	private static GatewayScope CreateGateway(string gatewayName) =>
		gatewayName switch
		{
			"memory" => CreateMemory(),
			"fake" => CreateFake(),
			_ => throw new ArgumentOutOfRangeException(
				nameof(gatewayName),
				gatewayName,
				"Unknown notification gateway."
			),
		};

	private static GatewayScope CreateMemory()
	{
		var gateway = new MemoryNotificationGateway();
		return new GatewayScope(gateway, () => gateway.Sent);
	}

	private static GatewayScope CreateFake()
	{
		var gateway = new FakeNotificationGateway();
		return new GatewayScope(gateway, () => gateway.Sent);
	}

	private static Notification TestNotification() =>
		new(
			NotificationId.New(),
			new NotificationRecipient("principal-1", "user@example.com", "User One"),
			new NotificationChannel("in-app"),
			"Subject",
			"Body",
			new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero)
		);
}
