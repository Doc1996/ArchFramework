using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Stores tracked command entries and records command lifecycle changes in the runtime log.
/// </summary>
public sealed class CommandBoard(IClock clock, RuntimeLog runtimeLog)
{
	private readonly SyncDictionary<CommandId, CommandEntry> _commandEntries = new();
	public IReadOnlyCollection<CommandEntry> CommandEntries => _commandEntries.Values();

	public CommandHandle Start(
		CommandId commandId,
		string name,
		IReadOnlyDictionary<string, object?>? properties = null
	)
	{
		if (_commandEntries.TryGet(commandId, out _))
			throw new InvalidOperationException($"Command '{commandId.Value}' is already tracked.");

		var utcNow = clock.UtcNow;
		var commandEntry = new CommandEntry(
			commandId,
			name,
			CommandStatus.Started,
			utcNow,
			utcNow,
			null,
			null,
			properties
		);

		_commandEntries.Set(commandId, commandEntry);
		LogCommand(commandEntry, "Command started.");
		return new CommandHandle(this, commandId);
	}

	public void Set(CommandEntry commandEntry)
	{
		_commandEntries.Set(commandEntry.Id, commandEntry);
		runtimeLog.Add(LogCategory.Command, "Command set.", commandEntry);
	}

	public bool TryGet(CommandId commandId, out CommandEntry? commandEntry) =>
		_commandEntries.TryGet(commandId, out commandEntry);

	public bool Clear(CommandId commandId)
	{
		var removed = _commandEntries.Remove(commandId);
		if (removed)
			runtimeLog.Add(LogCategory.Command, "Command cleared.", new { CommandId = commandId });

		return removed;
	}

	public void ClearAll()
	{
		_commandEntries.Clear();
		runtimeLog.Add(LogCategory.Command, "All commands cleared.");
	}

	public CommandEntry Complete(CommandId commandId) =>
		Finish(commandId, CommandStatus.Completed, null, "Command completed.");

	public CommandEntry Fail(CommandId commandId, string message) =>
		Fail(commandId, new Error("CommandFailed", message));

	public CommandEntry Fail(CommandId commandId, Error error) =>
		Finish(commandId, CommandStatus.Failed, error, "Command failed.");

	public CommandEntry Cancel(CommandId commandId) =>
		Finish(commandId, CommandStatus.Cancelled, null, "Command cancelled.");

	public CommandEntry Timeout(CommandId commandId) =>
		Finish(commandId, CommandStatus.TimedOut, null, "Command timed out.");

	private CommandEntry Finish(CommandId commandId, CommandStatus status, Error? error, string message)
	{
		if (!_commandEntries.TryGet(commandId, out var currentCommandEntry) || currentCommandEntry is null)
			throw new InvalidOperationException($"Command '{commandId.Value}' is not tracked.");

		var utcNow = clock.UtcNow;
		var commandEntry = currentCommandEntry with
		{
			Status = status,
			UpdatedAt = utcNow,
			FinishedAt = utcNow,
			Error = error,
		};

		_commandEntries.Set(commandId, commandEntry);
		LogCommand(commandEntry, message);
		return commandEntry;
	}

	private void LogCommand(CommandEntry commandEntry, string message)
	{
		runtimeLog.Add(
			LogCategory.Command,
			commandEntry.Status == CommandStatus.Failed ? TraceLevel.Error : TraceLevel.Info,
			message,
			new
			{
				CommandId = commandEntry.Id,
				CommandName = commandEntry.Name,
				commandEntry.Status,
				Error = commandEntry.Error?.Code,
			}
		);
	}
}
