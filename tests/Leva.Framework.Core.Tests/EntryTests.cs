using Leva.Framework.Core;
using Xunit;

namespace Leva.Framework.Core.Tests;

public sealed class EntryTests
{
	[Fact]
	public void AlarmEntry_StoresAlarmSnapshot()
	{
		var alarmId = AlarmId.New();
		var raisedAt = DateTimeOffset.UtcNow;
		var alarmEntry = new AlarmEntry(alarmId, "Door open", AlarmLevel.Warning, true, raisedAt);

		Assert.Equal(alarmId, alarmEntry.Id);
		Assert.Equal("Door open", alarmEntry.Message);
		Assert.Equal(AlarmLevel.Warning, alarmEntry.Level);
		Assert.True(alarmEntry.IsBlocking);
		Assert.Equal(raisedAt, alarmEntry.RaisedAt);
	}

	[Fact]
	public void CommandEntry_StoresLifecycleSnapshot()
	{
		var commandId = CommandId.New();
		var startedAt = DateTimeOffset.UtcNow;
		var commandEntry = new CommandEntry(commandId, "Move", CommandStatus.Started, startedAt, startedAt);

		Assert.Equal(commandId, commandEntry.Id);
		Assert.Equal("Move", commandEntry.Name);
		Assert.Equal(CommandStatus.Started, commandEntry.Status);
		Assert.Null(commandEntry.FinishedAt);
	}

	[Fact]
	public void StatusEntry_StoresLatestValue()
	{
		var updatedAt = DateTimeOffset.UtcNow;
		var statusEntry = new StatusEntry("Device", "Connected", true, updatedAt);

		Assert.Equal("Device", statusEntry.Source);
		Assert.Equal("Connected", statusEntry.Name);
		Assert.Equal(true, statusEntry.Value);
		Assert.Equal(updatedAt, statusEntry.UpdatedAt);
	}

	[Fact]
	public void TraceEntry_StoresDiagnosticItem()
	{
		var createdAt = DateTimeOffset.UtcNow;
		var traceEntry = new TraceEntry("Source", "Message", TraceLevel.Info, createdAt);

		Assert.Equal("Source", traceEntry.Source);
		Assert.Equal("Message", traceEntry.Message);
		Assert.Equal(TraceLevel.Info, traceEntry.Level);
		Assert.Equal(createdAt, traceEntry.CreatedAt);
	}

	[Fact]
	public void Snapshot_StoresStateTimeAndData()
	{
		var stateId = new StateId("Ready");
		var createdAt = DateTimeOffset.UtcNow;
		var snapshot = new Snapshot(stateId, createdAt, new Dictionary<string, object?> { ["Value"] = 123 });

		Assert.Equal(stateId, snapshot.StateId);
		Assert.Equal(createdAt, snapshot.CreatedAt);
		Assert.Equal(123, snapshot.Data["Value"]);
	}
}
