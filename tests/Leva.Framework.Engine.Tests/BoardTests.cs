using Leva.Framework.Core;
using Leva.Framework.Engine;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class BoardTests
{
	[Fact]
	public void AlarmBoard_RaisesAndClearsAlarms()
	{
		var clock = new FakeClock(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
		var runtimeLog = new RuntimeLog(clock, new FakeLogSink());
		var board = new AlarmBoard(clock, runtimeLog);

		var alarmId = AlarmId.New();
		board.Raise(new AlarmEntry(alarmId, "Blocked", AlarmLevel.Critical, true, default));

		Assert.True(board.HasAny);
		Assert.True(board.HasBlocking);
		Assert.Equal(clock.UtcNow, Assert.Single(board.AlarmEntries).RaisedAt);
		Assert.True(board.Clear(alarmId));
		Assert.False(board.HasAny);
	}

	[Fact]
	public void StatusBoard_SetsGetsAndClearsStatus()
	{
		var runtimeLog = new RuntimeLog(new FakeClock(), new FakeLogSink());
		var board = new StatusBoard(runtimeLog);

		var statusEntry = new StatusEntry("Device", "Connected", true, DateTimeOffset.UtcNow);
		board.Set(statusEntry);

		Assert.True(board.TryGet("Device", out var storedStatus));
		Assert.Equal(statusEntry, storedStatus);
		Assert.True(board.Clear("Device"));
		Assert.Empty(board.StatusEntries);
	}

	[Fact]
	public void CommandBoard_StartsCompletesAndClearsCommand()
	{
		var clock = new FakeClock(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
		var runtimeLog = new RuntimeLog(clock, new FakeLogSink());
		var board = new CommandBoard(clock, runtimeLog);

		var commandId = CommandId.New();
		var handle = board.Start(commandId, "Calibrate");
		clock.Advance(TimeSpan.FromSeconds(5));
		var completed = handle.Complete();

		Assert.Equal(CommandStatus.Completed, completed.Status);
		Assert.Equal(clock.UtcNow, completed.FinishedAt);
		Assert.True(board.TryGet(commandId, out _));
		Assert.True(board.Clear(commandId));
		Assert.Empty(board.CommandEntries);
	}

	[Fact]
	public void CommandBoard_FailStoresErrorAndLogsErrorLevel()
	{
		var logSink = new FakeLogSink();
		var clock = new FakeClock();
		var runtimeLog = new RuntimeLog(clock, logSink);
		var board = new CommandBoard(clock, runtimeLog);

		var commandId = CommandId.New();
		var failed = board.Start(commandId, "Move").Fail(new Error("MoveFailed", "Move failed."));

		Assert.Equal(CommandStatus.Failed, failed.Status);
		Assert.Equal("MoveFailed", failed.Error?.Code);
		Assert.Contains(logSink.LogEntries, logEntry => logEntry.Level == LogLevel.Error);
	}
}
