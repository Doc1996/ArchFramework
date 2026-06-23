using Leva.Framework.Core;
using Leva.Framework.Engine;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class RuntimeLogTests
{
	[Fact]
	public void Add_CreatesLogEntryAndMirrorsToLogSink()
	{
		var clock = new FakeClock(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
		var logSink = new FakeLogSink();
		var runtimeLog = new RuntimeLog(clock, logSink);
		var logEntry = runtimeLog.Add(LogCategory.Event, LogLevel.Warning, "Something happened.", new { Count = 5 });

		Assert.Equal(logEntry, Assert.Single(runtimeLog.LogEntries));
		Assert.Equal(LogLevel.Warning, logEntry.Level);
		Assert.Equal(5, logEntry.Properties?["Count"]);
		Assert.Equal("Something happened.", Assert.Single(logSink.LogEntries).Message);
	}

	[Fact]
	public void Add_ExtractsEventDetails()
	{
		var runtimeLog = new RuntimeLog(new FakeClock(), new FakeLogSink());
		var appEvent = new FakeEvent("Started");
		var logEntry = runtimeLog.Add(LogCategory.Event, "Event received.", appEvent);

		Assert.Equal(appEvent.Id.Value, logEntry.Properties?["EventId"]);
		Assert.Equal("Started", logEntry.Properties?["EventName"]);
	}
}
