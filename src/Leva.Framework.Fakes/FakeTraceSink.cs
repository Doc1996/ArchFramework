using Leva.Framework.Core;

namespace Leva.Framework.Fakes;

/// <summary>
/// Stores trace entries in memory for tests.
/// </summary>
public sealed class FakeTraceSink : ITraceSink
{
	private readonly List<TraceEntry> _traceEntries = new();
	public IReadOnlyList<TraceEntry> TraceEntries => _traceEntries.ToList();

	public void Write(TraceEntry traceEntry) => _traceEntries.Add(traceEntry);

	public void Clear() => _traceEntries.Clear();
}
