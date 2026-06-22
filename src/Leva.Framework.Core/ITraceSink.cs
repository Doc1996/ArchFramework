namespace Leva.Framework.Core;

/// <summary>
/// Receives diagnostic trace entries emitted by the runtime or application infrastructure.
/// </summary>
public interface ITraceSink
{
	void Write(TraceEntry traceEntry);
	void Clear();
}
