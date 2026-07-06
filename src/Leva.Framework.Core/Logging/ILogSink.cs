namespace Leva.Framework.Core;

/// <summary>
/// Receives structured runtime log entries emitted by the runtime or application infrastructure.
/// </summary>
public interface ILogSink
{
	void Write(LogEntry logEntry);
}
