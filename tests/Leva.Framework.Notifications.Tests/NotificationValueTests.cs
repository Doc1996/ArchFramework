using Leva.Framework.Notifications;
using Xunit;

namespace Leva.Framework.Notifications.Tests;

public sealed class NotificationValueTests
{
	[Fact]
	public void NotificationChannel_AllowsProviderDefinedChannels()
	{
		var channel = new NotificationChannel("signalr");
		Assert.Equal("signalr", channel.Value);
		Assert.Equal("signalr", channel.ToString());
	}

	[Fact]
	public void NotificationChannel_RejectsEmptyValues()
	{
		Assert.Throws<ArgumentException>(() => new NotificationChannel(""));
	}
}
