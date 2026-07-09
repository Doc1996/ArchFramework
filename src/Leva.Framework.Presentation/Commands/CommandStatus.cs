namespace Leva.Framework.Presentation;

/// <summary>
/// Describes the current state of one user-facing command or action.
/// </summary>
public enum CommandStatus
{
	Ready,
	Running,
	Succeeded,
	Failed,
	Disabled,
}
