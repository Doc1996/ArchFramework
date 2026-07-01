namespace Leva.Framework.Core;

/// <summary>
/// Describes the lifecycle state of a routine managed by the engine.
/// </summary>
public enum RoutineStatus
{
	NotStarted,
	Running,
	Completed,
	Cancelled,
	Failed,
}
