namespace Leva.Framework.Presentation;

/// <summary>
/// Message sink implementation that intentionally ignores presentation messages.
/// </summary>
public sealed class NullMessageSink : IMessageSink
{
	public void Write(MessageEntry message) { }
}
