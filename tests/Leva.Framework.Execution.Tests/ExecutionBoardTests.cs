using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Execution.Tests;

public sealed class ExecutionBoardTests
{
	[Fact]
	public async Task Runner_TracksEntryProgressAndLogs()
	{
		var clock = new FakeClock();
		var logSink = new FakeLogSink();
		var board = new ExecutionBoard(clock, logSink);

		var runner = new ExecutionRunner(board);
		var handle = runner.Start<string, string>("value", new EchoExecution());
		var result = await handle.Completion;

		Assert.True(result.IsSuccess);
		Assert.True(board.TryGet(handle.Id, out var entry));
		Assert.Equal(ExecutionStatus.Completed, entry?.Status);

		Assert.Equal("Echoing", entry?.ProgressMessage);
		Assert.Equal(100, entry?.ProgressPercent);
		Assert.Contains(logSink.LogEntries, entry => entry.Category == LogCategory.Execution);
	}

	[Fact]
	public async Task FailedExecution_StoresErrorAndFinishedAt()
	{
		var board = new ExecutionBoard(new FakeClock());
		var runner = new ExecutionRunner(board);
		var handle = runner.Start<string, string>("value", new FailingExecution());
		var result = await handle.Completion;

		Assert.True(result.IsFailure);
		Assert.True(board.TryGet(handle.Id, out var entry));

		Assert.Equal(ExecutionStatus.Failed, entry?.Status);
		Assert.Equal(result.Error, entry?.Error);
		Assert.True(entry?.FinishedAt.HasValue);
	}

	[Fact]
	public void Register_StoresExecutionName()
	{
		var board = new ExecutionBoard(new FakeClock());
		board.Register<string, string>(new EchoExecution());
		Assert.Contains(nameof(EchoExecution), board.RegisteredNames);
	}

	[Fact]
	public async Task ClearAll_RemovesEntries()
	{
		var board = new ExecutionBoard(new FakeClock());
		var runner = new ExecutionRunner(board);
		var handle = runner.Start<string, string>("value", new EchoExecution());

		await handle.Completion;
		board.ClearAll();
		Assert.Empty(board.Entries);
	}

	private sealed class EchoExecution : IExecution<string, string>
	{
		public Task<Result<string>> ExecuteAsync(
			string request,
			ExecutionReporter reporter,
			CancellationToken token = default
		)
		{
			reporter.Report("Echoing", 100);
			return Task.FromResult(Result<string>.Ok(request));
		}
	}

	private sealed class FailingExecution : IExecution<string, string>
	{
		public Task<Result<string>> ExecuteAsync(
			string request,
			ExecutionReporter reporter,
			CancellationToken token = default
		) => Task.FromResult(Result<string>.Fail(new Error("execution.test", "Test failure.")));
	}
}
