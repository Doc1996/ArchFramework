namespace Leva.Framework.Execution;

/// <summary>
/// Reports progress from the currently running execution.
/// </summary>
public sealed class ExecutionProgress
{
	private readonly ExecutionId _id;
	private readonly ExecutionBoard _board;

	internal ExecutionProgress(ExecutionId id, ExecutionBoard board)
	{
		_id = id;
		_board = board;
	}

	public void Report(string message, double? percent = null) => _board.Report(_id, message, percent);
}
