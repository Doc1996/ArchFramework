namespace Leva.Framework.Execution;

/// <summary>
/// Describes the lifecycle state of one execution attempt.
/// </summary>
public enum ExecutionStatus
{
	Running,
	Completed,
	Cancelled,
	Failed,
}
