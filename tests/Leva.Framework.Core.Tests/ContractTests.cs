using Leva.Framework.Core;
using Xunit;

namespace Leva.Framework.Core.Tests;

public sealed class ContractTests
{
	[Fact]
	public async Task StateContract_CanBeImplemented()
	{
		var state = new TestState();
		var access = new TestAccess();
		var appEvent = new TestEvent("Ping");

		await state.EnterAsync(access, CancellationToken.None);
		var handled = await state.HandleAsync(access, appEvent, CancellationToken.None);
		await state.ExitAsync(access, CancellationToken.None);

		Assert.True(handled);
		Assert.Equal(1, state.EnterCount);
		Assert.Equal(1, state.ExitCount);
	}

	private sealed record TestEvent(string Name) : IEvent
	{
		public EventId Id { get; } = EventId.New();
	}

	private sealed class TestAccess : IAccess
	{
		public ITransition Transition { get; } = new TestTransition();
		public ILogSink LogSink { get; } = new TestLogSink();
	}

	private sealed class TestTransition : ITransition
	{
		public void To(StateId stateId, string? reason = null) { }

		public void Reenter(string? reason = null) { }
	}

	private sealed class TestLogSink : ILogSink
	{
		public void Write(LogEntry logEntry) { }
	}

	private sealed class TestState : IState<TestAccess>
	{
		public StateId Id { get; } = new("Test");
		public string Name => "Test";
		public int EnterCount { get; private set; }
		public int ExitCount { get; private set; }

		public Task EnterAsync(TestAccess access, CancellationToken token)
		{
			EnterCount++;
			return Task.CompletedTask;
		}

		public Task ExitAsync(TestAccess access, CancellationToken token)
		{
			ExitCount++;
			return Task.CompletedTask;
		}

		public Task<bool> HandleAsync(TestAccess access, IEvent appEvent, CancellationToken token) =>
			Task.FromResult(true);
	}
}
