using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Log sink implementation that intentionally ignores log entries.
/// </summary>
public sealed class NullLogSink : ILogSink
{
	public void Write(LogEntry logEntry) { }

	public void Clear() { }
}
