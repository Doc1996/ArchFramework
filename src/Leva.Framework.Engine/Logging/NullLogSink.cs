using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Discarding log sink used as the default when no diagnostics output is configured.
/// </summary>
public sealed class NullLogSink : ILogSink
{
	public void Write(LogEntry logEntry) { }

	public void Clear() { }
}
