using Leva.Framework.Core;
using Xunit;

namespace Leva.Framework.Core.Tests;

public sealed class CoreValueTests
{
	[Fact]
	public void EventPriority_OrderMatchesDispatchPriority()
	{
		Assert.True(EventPriority.Critical > EventPriority.High);
		Assert.True(EventPriority.High > EventPriority.Normal);
		Assert.True(EventPriority.Normal > EventPriority.Low);
	}

	[Fact]
	public void AlarmEntry_CanCarryProperties()
	{
		var alarmEntry = new AlarmEntry(
			AlarmId.New(),
			"Alarm",
			AlarmLevel.Warning,
			false,
			DateTimeOffset.UnixEpoch,
			new Dictionary<string, object?> { ["Source"] = "Scheduler" }
		);

		Assert.Equal("Scheduler", alarmEntry.Properties?["Source"]);
	}

	[Fact]
	public void RuntimeIds_DefaultValuesAreDifferentFromNewValues()
	{
		Assert.NotEqual(default(AlarmId), AlarmId.New());
		Assert.NotEqual(default(EventId), EventId.New());
		Assert.NotEqual(default(RequestId), RequestId.New());
	}
}
