using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Represents one tracked command lifecycle returned to service or workflow code.
/// </summary>
public sealed class CommandHandle
{
	internal CommandHandle(CommandBoard commandBoard, CommandId id)
	{
		_commandBoard = commandBoard;
		Id = id;
	}

	private readonly CommandBoard _commandBoard;
	public CommandId Id { get; }

	public CommandEntry Complete() => _commandBoard.Complete(Id);

	public CommandEntry Fail(string message) => _commandBoard.Fail(Id, message);

	public CommandEntry Fail(Error error) => _commandBoard.Fail(Id, error);

	public CommandEntry Cancel() => _commandBoard.Cancel(Id);

	public CommandEntry Timeout() => _commandBoard.Timeout(Id);
}
