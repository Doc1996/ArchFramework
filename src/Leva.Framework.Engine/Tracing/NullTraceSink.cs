using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Discarding trace sink used as the default when no diagnostics output is configured.
/// </summary>
public sealed class NullTraceSink : ITraceSink
{
	public void Write(TraceEntry traceEntry) { }

	public void Clear() { }
}
