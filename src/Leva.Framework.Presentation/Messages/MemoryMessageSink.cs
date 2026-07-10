using Leva.Framework.Core;

namespace Leva.Framework.Presentation;

/// <summary>
/// Stores presentation message entries in memory for inspection, demos, or tests.
/// </summary>
public sealed class MemoryMessageSink : IMessageSink
{
	private readonly SyncList<MessageEntry> _messageEntries = new();
	public IReadOnlyList<MessageEntry> MessageEntries => _messageEntries.List();

	public void Write(MessageEntry message) => _messageEntries.Add(message);

	public void Clear() => _messageEntries.Clear();
}
