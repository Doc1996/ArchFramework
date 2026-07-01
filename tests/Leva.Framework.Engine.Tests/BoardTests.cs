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

}