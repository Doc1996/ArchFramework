using Leva.Framework.Core;

namespace Leva.Framework.Execution;

/// <summary>
/// Starts execution contracts on background tasks and coordinates status, progress, cancellation, and result.
/// </summary>
public sealed class ExecutionRunner(ExecutionBoard board)
{
	private const string UnregisteredExecutionName = "UnregisteredExecution";

	public ExecutionHandle<TResult> Start<TRequest, TResult>(TRequest request, CancellationToken token = default)
	{
		if (board.TryGetExecution<TRequest, TResult>(out var execution) && execution is not null)
			return Start(request, execution, token);

		var error = ExecutionErrors.NotRegistered(typeof(TRequest), typeof(TResult));
		var executionId = ExecutionId.New();
		var task = Task.FromResult(Result<TResult>.Fail(error));

		board.Start(executionId, UnregisteredExecutionName);
		board.Fail(executionId, error);
		return new ExecutionHandle<TResult>(executionId, board, task, static () => { });
	}

	public ExecutionHandle<TResult> Start<TRequest, TResult>(
		TRequest request,
		IExecution<TRequest, TResult> execution,
		CancellationToken token = default
	)
	{
		ArgumentNullException.ThrowIfNull(execution);

		var executionId = ExecutionId.New();
		var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
		var progress = new ExecutionProgress(executionId, board);

		board.Start(executionId, execution.GetType().Name);
		var completion = Task.Run(() => RunAsync(request, execution, progress, executionId, tokenSource));
		return new ExecutionHandle<TResult>(executionId, board, completion, () => CancelExecution(tokenSource));
	}

	private async Task<Result<TResult>> RunAsync<TRequest, TResult>(
		TRequest request,
		IExecution<TRequest, TResult> execution,
		ExecutionProgress progress,
		ExecutionId executionId,
		CancellationTokenSource tokenSource
	)
	{
		try
		{
			tokenSource.Token.ThrowIfCancellationRequested();
			var result = await execution.ExecuteAsync(request, progress, tokenSource.Token);

			if (tokenSource.IsCancellationRequested)
				return MarkCancelled<TResult>(executionId);
			return Finish(executionId, result);
		}
		catch (OperationCanceledException) when (tokenSource.IsCancellationRequested)
		{
			return MarkCancelled<TResult>(executionId);
		}
		catch (Exception exception)
		{
			return MarkFailed<TResult>(executionId, exception);
		}
		finally
		{
			tokenSource.Dispose();
		}
	}

	private Result<TResult> Finish<TResult>(ExecutionId executionId, Result<TResult> result)
	{
		if (result.IsSuccess)
			board.Complete(executionId);
		else
			board.Fail(executionId, result.Error);

		return result;
	}

	private Result<TResult> MarkCancelled<TResult>(ExecutionId executionId)
	{
		var error = ExecutionErrors.Cancelled(executionId);
		board.Cancel(executionId);
		return Result<TResult>.Fail(error);
	}

	private Result<TResult> MarkFailed<TResult>(ExecutionId executionId, Exception exception)
	{
		var error = ExecutionErrors.Failed(executionId, exception);
		board.Fail(executionId, error);
		return Result<TResult>.Fail(error);
	}

	private static void CancelExecution(CancellationTokenSource tokenSource)
	{
		try
		{
			tokenSource.Cancel();
		}
		catch (ObjectDisposedException) { }
	}
}
