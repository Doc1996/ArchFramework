using Leva.Framework.Presentation;
using Xunit;

namespace Leva.Framework.Presentation.Tests;

public sealed class MessageSinkTests
{
	[Fact]
	public void MemoryMessageSink_StoresMessages()
	{
		var sink = new MemoryMessageSink();
		sink.Write(MessageEntry.Info("Loading."));
		sink.Write(MessageEntry.Success("Saved."));

		Assert.Equal(2, sink.MessageEntries.Count);
		Assert.Equal("Loading.", sink.MessageEntries[0].Text);
		Assert.Equal("Saved.", sink.MessageEntries[1].Text);
	}

	[Fact]
	public void MemoryMessageSink_Clear_RemovesMessages()
	{
		var sink = new MemoryMessageSink();
		sink.Write(MessageEntry.Info("Loading."));
		sink.Clear();

		Assert.Empty(sink.MessageEntries);
	}

	[Fact]
	public void NullMessageSink_IgnoresMessages()
	{
		var sink = new NullMessageSink();
		var exception = Record.Exception(() => sink.Write(MessageEntry.Error("Ignored.")));
		Assert.Null(exception);
	}
}
