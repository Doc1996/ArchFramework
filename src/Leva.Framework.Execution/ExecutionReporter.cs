namespace Leva.Framework.Execution;

/// <summary>
/// Reports progress from the currently running execution.
/// </summary>
public sealed class ExecutionReporter
{
	private readonly ExecutionBoard _board;
	private readonly ExecutionId _executionId;

	internal ExecutionReporter(ExecutionBoard board, ExecutionId executionId)
	{
		_board = board;
		_executionId = executionId;
	}

	public void Report(string message, double? percent = null) => _board.Report(_executionId, message, percent);
}
