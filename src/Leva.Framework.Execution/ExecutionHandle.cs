using Leva.Framework.Core;

namespace Leva.Framework.Execution;

/// <summary>
/// Represents one running execution and exposes status, cancellation, and typed completion result.
/// </summary>
public sealed class ExecutionHandle<TResult>
{
	private readonly ExecutionBoard _board;
	private readonly Action _cancelExecution;

	internal ExecutionHandle(
		ExecutionId id,
		Task<Result<TResult>> completion,
		ExecutionBoard board,
		Action cancelExecution
	)
	{
		Id = id;
		Completion = completion;
		_board = board;
		_cancelExecution = cancelExecution;
	}

	public ExecutionId Id { get; }
	public Task<Result<TResult>> Completion { get; }
	public ExecutionEntry? Entry => _board.TryGet(Id, out var entry) ? entry : null;
	public ExecutionStatus? Status => Entry?.Status;

	public void Cancel() => _cancelExecution();
}
