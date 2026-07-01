using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Execution.Tests;

public sealed class ExecutionRunnerTests
{
	[Fact]
	public async Task Start_RunsExecutionAndCompletesWithResult()
	{
		var board = new ExecutionBoard(new FakeClock());
		var runner = new ExecutionRunner(board);
		var handle = runner.Start<int, int>(21, new MultiplyExecution());
		var result = await handle.Completion;

		Assert.True(result.IsSuccess);
		Assert.Equal(42, result.Value);
		Assert.Equal(ExecutionStatus.Completed, handle.Status);

		Assert.Equal("Multiplying", handle.Entry?.ProgressMessage);
		Assert.Equal(100, handle.Entry?.ProgressPercent);
	}

	[Fact]
	public async Task Start_FromBoard_RunsRegisteredExecution()
	{
		var board = new ExecutionBoard(new FakeClock()).Register<int, int>(new MultiplyExecution());
		var runner = new ExecutionRunner(board);
		var handle = runner.Start<int, int>(10);
		var result = await handle.Completion;

		Assert.True(result.IsSuccess);
		Assert.Equal(20, result.Value);
		Assert.Equal(ExecutionStatus.Completed, handle.Status);
	}

	[Fact]
	public async Task Start_ReturnsFailureWhenExecutionIsNotRegistered()
	{
		var board = new ExecutionBoard(new FakeClock());
		var runner = new ExecutionRunner(board);
		var handle = runner.Start<int, int>(10);
		var result = await handle.Completion;

		Assert.True(result.IsFailure);
		Assert.Equal("execution.not_registered", result.Error.Code);
		Assert.Equal(ExecutionStatus.Failed, handle.Status);
	}

	[Fact]
	public async Task Cancel_CancelsRunningExecution()
	{
		var board = new ExecutionBoard(new FakeClock());
		var runner = new ExecutionRunner(board);
		var execution = new WaitingExecution();
		var handle = runner.Start<string, string>("value", execution);

		await execution.Started.Task;
		handle.Cancel();
		var result = await handle.Completion;

		Assert.True(result.IsFailure);
		Assert.Equal("execution.cancelled", result.Error.Code);
		Assert.Equal(ExecutionStatus.Cancelled, handle.Status);
	}

	[Fact]
	public async Task Failure_ReturnsFailureAndTracksError()
	{
		var board = new ExecutionBoard(new FakeClock());
		var runner = new ExecutionRunner(board);
		var handle = runner.Start<string, string>("value", new FailingExecution());
		var result = await handle.Completion;

		Assert.True(result.IsFailure);
		Assert.Equal("execution.failed", result.Error.Code);
		Assert.Equal(ExecutionStatus.Failed, handle.Status);
		Assert.Equal(result.Error, handle.Entry?.Error);
	}

	private sealed class MultiplyExecution : IExecution<int, int>
	{
		public Task<Result<int>> ExecuteAsync(
			int request,
			ExecutionReporter reporter,
			CancellationToken token = default
		)
		{
			reporter.Report("Multiplying", 100);
			return Task.FromResult(Result<int>.Ok(request * 2));
		}
	}

	private sealed class WaitingExecution : IExecution<string, string>
	{
		public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public async Task<Result<string>> ExecuteAsync(
			string request,
			ExecutionReporter reporter,
			CancellationToken token = default
		)
		{
			Started.SetResult();
			await Task.Delay(TimeSpan.FromMinutes(1), token);
			return Result<string>.Ok(request);
		}
	}

	private sealed class FailingExecution : IExecution<string, string>
	{
		public Task<Result<string>> ExecuteAsync(
			string request,
			ExecutionReporter reporter,
			CancellationToken token = default
		) => throw new InvalidOperationException("Failure.");
	}
}
