using Leva.Framework.Notifications;
using Leva.Framework.Notifications.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Leva.Framework.Notifications.SignalR.Tests;

public sealed class SignalRNotificationServiceTests
{
	[Fact]
	public void AddSignalRNotifications_RegistersGateway()
	{
		var services = new ServiceCollection();
		services.AddSignalRNotifications();

		using var provider = services.BuildServiceProvider();
		Assert.NotNull(provider.GetService<SignalRNotificationGateway>());
		Assert.NotNull(provider.GetService<INotificationGateway>());
	}

	[Fact]
	public void AddSignalRNotifications_AllowsCustomChannel()
	{
		var services = new ServiceCollection();
		services.AddSignalRNotifications(options => options.Channel = new NotificationChannel("browser"));

		using var provider = services.BuildServiceProvider();
		Assert.NotNull(provider.GetRequiredService<INotificationGateway>());
	}
}
