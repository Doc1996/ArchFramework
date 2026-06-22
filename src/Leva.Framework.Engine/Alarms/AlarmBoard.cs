using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Stores the current active alarms and records alarm changes in the runtime log.
/// </summary>
public sealed class AlarmBoard(IClock clock, RuntimeLog runtimeLog)
{
	private readonly SyncDictionary<AlarmId, AlarmEntry> _alarmEntries = new();
	public IReadOnlyCollection<AlarmEntry> AlarmEntries => _alarmEntries.Values();

	public bool HasAny => _alarmEntries.Any();
	public bool HasBlocking => _alarmEntries.Any(alarmEntry => alarmEntry.IsBlocking);

	public void Raise(AlarmEntry alarmEntry)
	{
		if (alarmEntry.RaisedAt == default)
			alarmEntry = alarmEntry with { RaisedAt = clock.UtcNow };

		_alarmEntries.Set(alarmEntry.Id, alarmEntry);
		runtimeLog.Add(LogCategory.Alarm, "Alarm raised.", alarmEntry);
	}

	public bool Clear(AlarmId alarmId)
	{
		var removed = _alarmEntries.Remove(alarmId);
		if (removed)
			runtimeLog.Add(LogCategory.Alarm, "Alarm cleared.", new { AlarmId = alarmId });

		return removed;
	}

	public void ClearAll()
	{
		if (_alarmEntries.ClearAndHadAny())
			runtimeLog.Add(LogCategory.Alarm, "All alarms cleared.");
	}
}
