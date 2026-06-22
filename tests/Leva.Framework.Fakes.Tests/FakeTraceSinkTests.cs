using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Fakes.Tests;

public sealed class FakeTraceSinkTests
{
	[Fact]
	public void Write_StoresTraceEntry()
	{
		var sink = new FakeTraceSink();
		var entry = new TraceEntry("Source", "Message", TraceLevel.Info, DateTimeOffset.UtcNow);

		sink.Write(entry);
		Assert.Equal(entry, Assert.Single(sink.TraceEntries));
	}

	[Fact]
	public void Clear_RemovesEntries()
	{
		var sink = new FakeTraceSink();
		sink.Write(new TraceEntry("Source", "Message", TraceLevel.Info, DateTimeOffset.UtcNow));

		sink.Clear();
		Assert.Empty(sink.TraceEntries);
	}
}
