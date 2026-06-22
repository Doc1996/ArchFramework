using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Stores latest known status entries so later states can read current external or service state.
/// </summary>
public sealed class StatusBoard(RuntimeLog runtimeLog)
{
	private readonly SyncDictionary<string, StatusEntry> _statusEntries = new();
	public IReadOnlyCollection<StatusEntry> StatusEntries => _statusEntries.Values();

	public void Set(StatusEntry statusEntry)
	{
		_statusEntries.Set(statusEntry.Source, statusEntry);
		runtimeLog.Add(LogCategory.Status, "Status set.", statusEntry);
	}

	public bool TryGet(string source, out StatusEntry? statusEntry) => _statusEntries.TryGet(source, out statusEntry);

	public bool Clear(string source)
	{
		var removed = _statusEntries.Remove(source);
		if (removed)
			runtimeLog.Add(LogCategory.Status, "Status cleared.", new { Source = source });

		return removed;
	}

	public void ClearAll()
	{
		if (_statusEntries.ClearAndHadAny())
			runtimeLog.Add(LogCategory.Status, "All statuses cleared.");
	}
}
