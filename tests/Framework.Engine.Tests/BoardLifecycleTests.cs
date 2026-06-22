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
		var runtimeLog = new RuntimeLog(clock, new FakeTraceSink());
		var board = new AlarmBoard(clock, runtimeLog);
		var cleared = board.Clear(AlarmId.New());

		Assert.False(cleared);
		Assert.DoesNotContain(runtimeLog.LogEntries, entry => entry.Message == "Alarm cleared.");
	}

	[Fact]
	public void AlarmBoard_ClearAll_LogsOnlyWhenAlarmsExisted()
	{
		var clock = new FakeClock();
		var runtimeLog = new RuntimeLog(clock, new FakeTraceSink());
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
		var runtimeLog = new RuntimeLog(new FakeClock(), new FakeTraceSink());
		var board = new StatusBoard(runtimeLog);

		board.ClearAll();
		Assert.DoesNotContain(runtimeLog.LogEntries, entry => entry.Message == "All statuses cleared.");

		board.Set(new StatusEntry("Device", "Mode", "Auto", DateTimeOffset.UnixEpoch));
		board.ClearAll();

		Assert.Empty(board.StatusEntries);
		Assert.Contains(runtimeLog.LogEntries, entry => entry.Message == "All statuses cleared.");
	}

	[Fact]
	public void CommandBoard_ClearAll_LogsOnlyWhenCommandsExisted()
	{
		var clock = new FakeClock();
		var runtimeLog = new RuntimeLog(clock, new FakeTraceSink());
		var board = new CommandBoard(clock, runtimeLog);

		board.Start(CommandId.New(), "Command");
		board.ClearAll();

		Assert.Empty(board.CommandEntries);
		Assert.Contains(runtimeLog.LogEntries, entry => entry.Message == "All commands cleared.");
	}

	[Fact]
	public void CommandBoard_StartDuplicate_Throws()
	{
		var board = CreateCommandBoard(out _);
		var commandId = CommandId.New();

		board.Start(commandId, "Command");
		Assert.Throws<InvalidOperationException>(() => board.Start(commandId, "Command"));
	}

	[Fact]
	public void CommandBoard_CancelAndTimeout_UpdateLifecycle()
	{
		var clock = new FakeClock();
		var runtimeLog = new RuntimeLog(clock, new FakeTraceSink());
		var board = new CommandBoard(clock, runtimeLog);
		var cancelId = CommandId.New();
		var timeoutId = CommandId.New();

		var cancelled = board.Start(cancelId, "Cancel").Cancel();
		clock.Advance(TimeSpan.FromSeconds(1));
		var timedOut = board.Start(timeoutId, "Timeout").Timeout();

		Assert.Equal(CommandStatus.Cancelled, cancelled.Status);
		Assert.Equal(CommandStatus.TimedOut, timedOut.Status);
		Assert.NotNull(cancelled.FinishedAt);
		Assert.Equal(clock.UtcNow, timedOut.FinishedAt);
	}

	[Fact]
	public void CommandBoard_Set_ReplacesExistingEntry()
	{
		var clock = new FakeClock();
		var runtimeLog = new RuntimeLog(clock, new FakeTraceSink());
		var board = new CommandBoard(clock, runtimeLog);
		var commandId = CommandId.New();
		var first = new CommandEntry(commandId, "Command", CommandStatus.Started, clock.UtcNow, clock.UtcNow);
		var second = first with { Status = CommandStatus.Completed, FinishedAt = clock.UtcNow };

		board.Set(first);
		board.Set(second);

		Assert.True(board.TryGet(commandId, out var stored));
		Assert.Equal(CommandStatus.Completed, stored?.Status);
	}

	private static CommandBoard CreateCommandBoard(out RuntimeLog runtimeLog)
	{
		var clock = new FakeClock();
		runtimeLog = new RuntimeLog(clock, new FakeTraceSink());
		return new CommandBoard(clock, runtimeLog);
	}
}
