using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Fakes.Tests;

public sealed class FakeLogSinkTests
{
	[Fact]
	public void Write_StoresLogEntry()
	{
		var sink = new FakeLogSink();
		var entry = new LogEntry("Source", "Message", LogCategory.Event, LogLevel.Info, DateTimeOffset.UtcNow);

		sink.Write(entry);
		Assert.Equal(entry, Assert.Single(sink.LogEntries));
	}

	[Fact]
	public void Clear_RemovesEntries()
	{
		var sink = new FakeLogSink();
		sink.Write(new LogEntry("Source", "Message", LogCategory.Event, LogLevel.Info, DateTimeOffset.UtcNow));

		sink.Clear();
		Assert.Empty(sink.LogEntries);
	}
}
