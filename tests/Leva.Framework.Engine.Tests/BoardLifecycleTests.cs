using Leva.Framework.Core;
using Leva.Framework.Engine;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class BoardLifecycleTests
{
	[Fact]
	public void AlarmBoard_ClearMissing_ReturnsFalseAndDoesNotLogClear()
	{
		var clock = new FakeClock();
		var runtimeLog = new RuntimeLog(clock, new FakeLogSink());
		var board = new AlarmBoard(clock, runtimeLog);
		var cleared = board.Clear(AlarmId.New());

		Assert.False(cleared);
		Assert.DoesNotContain(runtimeLog.LogEntries, entry => entry.Message == "Alarm cleared.");
	}

	[Fact]
	public void AlarmBoard_ClearAll_LogsOnlyWhenAlarmsExisted()
	{
		var clock = new FakeClock();
		var runtimeLog = new RuntimeLog(clock, new FakeLogSink());
		var board = new AlarmBoard(clock, runtimeLog);

		board.ClearAll();
		Assert.DoesNotContain(runtimeLog.LogEntries, entry => entry.Message == "All alarms cleared.");

		board.Raise(new AlarmEntry(AlarmId.New(), "Alarm", AlarmLevel.Warning, false, clock.UtcNow));
		board.ClearAll();

		Assert.Empty(board.AlarmEntries);
		Assert.Contains(runtimeLog.LogEntries, entry => entry.Message == "All alarms cleared.");
	}

	[Fact]
	public void StatusBoard_ClearAll_LogsOnlyWhenStatusesExisted()
	{
		var runtimeLog = new RuntimeLog(new FakeClock(), new FakeLogSink());
		var board = new StatusBoard(runtimeLog);

		board.ClearAll();
		Assert.DoesNotContain(runtimeLog.LogEntries, entry => entry.Message == "All statuses cleared.");

		board.Set(new StatusEntry("Device", "Mode", "Auto", DateTimeOffset.UnixEpoch));
		board.ClearAll();

		Assert.Empty(board.StatusEntries);
		Assert.Contains(runtimeLog.LogEntries, entry => entry.Message == "All statuses cleared.");
	}
}
