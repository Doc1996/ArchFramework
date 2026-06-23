using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Stores diagnostic log entries in memory for inspection, demos, or tests.
/// </summary>
public sealed class MemoryLogSink : ILogSink
{
	private readonly SyncList<LogEntry> _logEntries = new();
	public IReadOnlyList<LogEntry> LogEntries => _logEntries.List();

	public void Write(LogEntry logEntry) => _logEntries.Add(logEntry);

	public void Clear() => _logEntries.Clear();
}
