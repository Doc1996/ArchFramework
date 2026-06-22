namespace Leva.Framework.Core;

/// <summary>
/// Describes the known runtime state of a command issued by application logic.
/// </summary>
public enum CommandStatus
{
	Started,
	Completed,
	Cancelled,
	TimedOut,
	Failed,
	Unknown,
}
