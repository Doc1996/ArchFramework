using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class StateMachineTests
{
	[Fact]
	public async Task StartAsync_EntersInitialState()
	{
		var readyId = new StateId("Ready");
		var ready = new FakeState(readyId);
		var fake = new FakeContext();
		var context = CreateContext(fake, [ready]);

		await context.StateMachine.StartAsync(readyId);
		Assert.Equal(readyId, context.StateMachine.CurrentStateId);
		Assert.Equal(1, ready.EnterCount);
	}

	[Fact]
	public async Task StateCanRequestTransition()
	{
		var firstId = new StateId("First");
		var secondId = new StateId("Second");
		var first = new FakeState(
			firstId,
			enter: (access, token) =>
			{
				access.Transition.To(secondId, "continue");
				return Task.CompletedTask;
			}
		);

		var second = new FakeState(secondId);
		var fake = new FakeContext();
		var context = CreateContext(fake, [first, second]);

		await context.StateMachine.StartAsync(firstId);
		Assert.Equal(secondId, context.StateMachine.CurrentStateId);
		Assert.Equal(1, first.ExitCount);
		Assert.Equal(1, second.EnterCount);
	}

	[Fact]
	public async Task HandleAsync_RoutesEventToCurrentState()
	{
		var stateId = new StateId("Ready");
		var state = new FakeState(stateId, handle: (_, appEvent, _) => Task.FromResult(appEvent.Name == "Ping"));
		var fake = new FakeContext();
		var context = CreateContext(fake, [state]);

		await context.StateMachine.StartAsync(stateId);
		await context.EventDispatcher.DispatchAsync(new FakeEvent("Ping"), CancellationToken.None);

		Assert.Equal(1, state.HandleCount);
		Assert.Contains(context.RuntimeLog.LogEntries, entry => entry.Message == "Event handled by state.");
	}

	[Fact]
	public async Task StartAsync_ThrowsForUnknownState()
	{
		var fake = new FakeContext();
		var context = CreateContext(fake, []);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			context.StateMachine.StartAsync(new StateId("Missing"))
		);
	}

	private static Context CreateContext(FakeContext fake, IEnumerable<IState<FakeAccess>> states)
	{
		var builder = new ContextBuilder()
			.WithClock(fake.Clock)
			.WithTraceSink(fake.TraceSink)
			.WithEventQueue(fake.EventQueue);

		foreach (var state in states)
			builder.AddState(state, access => new FakeAccess(access));
		return builder.Build();
	}
}
