namespace Leva.Framework.Presentation;

/// <summary>
/// Stores the current presentation state for one user-facing command or action.
/// </summary>
public sealed record CommandState(CommandStatus Status, string? Label = null, MessageEntry? Message = null)
{
	public bool CanRun => Status == CommandStatus.Ready;
	public bool IsRunning => Status == CommandStatus.Running;
	public bool HasFailed => Status == CommandStatus.Failed;

	public static CommandState Ready(string? label = null) => new(CommandStatus.Ready, label);

	public static CommandState Running(string? label = null, string? message = null) =>
		new(CommandStatus.Running, label, message is null ? null : MessageEntry.Info(message));

	public static CommandState Succeeded(string? label = null, string? message = null) =>
		new(CommandStatus.Succeeded, label, message is null ? null : MessageEntry.Success(message));

	public static CommandState Failed(string message, string? label = null, string? code = null) =>
		new(CommandStatus.Failed, label, MessageEntry.Error(message, code: code));

	public static CommandState Failed(MessageEntry message, string? label = null) =>
		new(CommandStatus.Failed, label, message);

	public static CommandState Disabled(string? label = null, string? message = null) =>
		new(CommandStatus.Disabled, label, message is null ? null : MessageEntry.Info(message));
}
