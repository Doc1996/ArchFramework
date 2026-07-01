using Leva.Framework.Core;

namespace Leva.Framework.Execution;

/// <summary>
/// Represents the caller side of one started execution.
/// </summary>
public sealed class ExecutionHandle<TResult>
{
	private readonly ExecutionBoard _board;
	private readonly Action _cancelExecution;

	internal ExecutionHandle(
		ExecutionId id,
		ExecutionBoard board,
		Task<Result<TResult>> completion,
		Action cancelExecution
	)
	{
		Id = id;
		_board = board;
		Completion = completion;
		_cancelExecution = cancelExecution;
	}

	public ExecutionId Id { get; }
	public Task<Result<TResult>> Completion { get; }
	public ExecutionEntry? Entry => _board.TryGet(Id, out var entry) ? entry : null;
	public ExecutionStatus? Status => Entry?.Status;

	/// <summary>
	/// Requests cooperative cancellation for this execution.
	/// </summary>
	public void Cancel() => _cancelExecution();
}
