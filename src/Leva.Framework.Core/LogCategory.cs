namespace Leva.Framework.Core;

/// <summary>
/// Groups runtime log entries by broad framework area.
/// </summary>
public enum LogCategory
{
	Event,
	State,
	Transition,
	Routine,
	Behavior,
	Alarm,
	Status,
	Execution,
	Snapshot,
}
