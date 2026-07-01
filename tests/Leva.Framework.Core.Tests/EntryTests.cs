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
	public void LogEntry_StoresDiagnosticItem()
	{
		var createdAt = DateTimeOffset.UtcNow;
		var logEntry = new LogEntry("Source", "Message", LogCategory.Event, LogLevel.Info, createdAt);

		Assert.Equal("Source", logEntry.Source);
		Assert.Equal("Message", logEntry.Message);
		Assert.Equal(LogLevel.Info, logEntry.Level);
		Assert.Equal(createdAt, logEntry.CreatedAt);
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
