namespace Leva.Framework.Engine;

/// <summary>
/// Groups runtime log entries by broad engine area.
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
	Command,
	Snapshot,
}
