using Leva.Framework.Core;
using Leva.Framework.Engine;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class RuntimeLogDetailTests
{
	[Fact]
	public void Add_MergesDictionaryDetails()
	{
		var runtimeLog = new RuntimeLog(new FakeClock(), new FakeLogSink());
		var details = new Dictionary<string, object?> { ["A"] = 1, ["B"] = "two" };
		var logEntry = runtimeLog.Add(LogCategory.Event, "Message.", details);

		Assert.Equal(1, logEntry.Properties?["A"]);
		Assert.Equal("two", logEntry.Properties?["B"]);
	}

	[Fact]
	public void Add_NormalizesIdValues()
	{
		var runtimeLog = new RuntimeLog(new FakeClock(), new FakeLogSink());
		var commandId = CommandId.New();
		var logEntry = runtimeLog.Add(LogCategory.Command, "Command.", commandId);

		Assert.Equal(commandId.Value, logEntry.Properties?["CommandId"]);
	}

	[Fact]
	public void Add_UsesExplicitLogLevelInMirroredLogEntry()
	{
		var logSink = new FakeLogSink();
		var runtimeLog = new RuntimeLog(new FakeClock(), logSink);
		runtimeLog.Add(LogCategory.Alarm, LogLevel.Error, "Alarm.");

		Assert.Equal(LogLevel.Error, Assert.Single(runtimeLog.LogEntries).Level);
		Assert.Equal(LogLevel.Error, Assert.Single(logSink.LogEntries).Level);
	}

	[Fact]
	public void Clear_RemovesLogEntriesOnly()
	{
		var logSink = new FakeLogSink();
		var runtimeLog = new RuntimeLog(new FakeClock(), logSink);
		runtimeLog.Add(LogCategory.Event, "Message.");
		runtimeLog.Clear();

		Assert.Empty(runtimeLog.LogEntries);
		Assert.NotEmpty(logSink.LogEntries);
	}
}
