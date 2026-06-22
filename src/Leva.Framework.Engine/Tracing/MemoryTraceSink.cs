using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Stores diagnostic trace entries in memory for inspection, demos, or tests.
/// </summary>
public sealed class MemoryTraceSink : ITraceSink
{
	private readonly SyncList<TraceEntry> _traceEntries = new();
	public IReadOnlyList<TraceEntry> TraceEntries => _traceEntries.List();

	public void Write(TraceEntry traceEntry) => _traceEntries.Add(traceEntry);

	public void Clear() => _traceEntries.Clear();
}
