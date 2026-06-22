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
		var runtimeLog = new RuntimeLog(new FakeClock(), new FakeTraceSink());
		var details = new Dictionary<string, object?> { ["A"] = 1, ["B"] = "two" };
		var logEntry = runtimeLog.Add(LogCategory.Event, "Message.", details);

		Assert.Equal(1, logEntry.Properties?["A"]);
		Assert.Equal("two", logEntry.Properties?["B"]);
	}

	[Fact]
	public void Add_NormalizesIdValues()
	{
		var runtimeLog = new RuntimeLog(new FakeClock(), new FakeTraceSink());
		var commandId = CommandId.New();
		var logEntry = runtimeLog.Add(LogCategory.Command, "Command.", commandId);

		Assert.Equal(commandId.Value, logEntry.Properties?["CommandId"]);
	}

	[Fact]
	public void Add_UsesExplicitTraceLevelInMirroredTraceEntry()
	{
		var traceSink = new FakeTraceSink();
		var runtimeLog = new RuntimeLog(new FakeClock(), traceSink);
		runtimeLog.Add(LogCategory.Alarm, TraceLevel.Error, "Alarm.");

		Assert.Equal(TraceLevel.Error, Assert.Single(runtimeLog.LogEntries).Level);
		Assert.Equal(TraceLevel.Error, Assert.Single(traceSink.TraceEntries).Level);
	}

	[Fact]
	public void Clear_RemovesLogEntriesOnly()
	{
		var traceSink = new FakeTraceSink();
		var runtimeLog = new RuntimeLog(new FakeClock(), traceSink);
		runtimeLog.Add(LogCategory.Event, "Message.");
		runtimeLog.Clear();

		Assert.Empty(runtimeLog.LogEntries);
		Assert.NotEmpty(traceSink.TraceEntries);
	}
}
