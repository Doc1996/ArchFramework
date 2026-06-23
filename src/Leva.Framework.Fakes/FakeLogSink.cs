using Leva.Framework.Core;

namespace Leva.Framework.Fakes;

/// <summary>
/// Stores log entries in memory for tests.
/// </summary>
public sealed class FakeLogSink : ILogSink
{
	private readonly List<LogEntry> _logEntries = new();
	public IReadOnlyList<LogEntry> LogEntries => _logEntries.ToList();

	public void Write(LogEntry logEntry) => _logEntries.Add(logEntry);

	public void Clear() => _logEntries.Clear();
}
