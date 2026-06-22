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
	public void CommandEntry_CanRepresentFinishedFailure()
	{
		var error = new Error("Failed", "Command failed.");
		var finishedAt = DateTimeOffset.UnixEpoch.AddSeconds(5);

		var commandEntry = new CommandEntry(
			CommandId.New(),
			"Command",
			CommandStatus.Failed,
			DateTimeOffset.UnixEpoch,
			finishedAt,
			finishedAt,
			error
		);

		Assert.Equal(CommandStatus.Failed, commandEntry.Status);
		Assert.Equal(finishedAt, commandEntry.FinishedAt);
		Assert.Equal(error, commandEntry.Error);
	}

	[Fact]
	public void RuntimeIds_DefaultValuesAreDifferentFromNewValues()
	{
		Assert.NotEqual(default(AlarmId), AlarmId.New());
		Assert.NotEqual(default(CommandId), CommandId.New());
		Assert.NotEqual(default(EventId), EventId.New());
		Assert.NotEqual(default(RequestId), RequestId.New());
	}
}
