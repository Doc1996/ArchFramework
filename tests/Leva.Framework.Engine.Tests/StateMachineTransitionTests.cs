using Leva.Framework.Core;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class StateMachineTransitionTests
{
	[Fact]
	public async Task Reenter_ExitsAndEntersCurrentStateAgain()
	{
		var state = new FakeState(
			new StateId("Ready"),
			handle: (access, _, _) =>
			{
				access.Transition.Reenter("reload");
				return Task.FromResult(true);
			}
		);

		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [state]);
		await context.StateMachine.StartAsync(state.Id);
		await context.EventDispatcher.DispatchAsync(new FakeEvent("Reload"), CancellationToken.None);

		Assert.Equal(state.Id, context.StateMachine.CurrentStateId);
		Assert.Equal(2, state.EnterCount);
		Assert.Equal(1, state.ExitCount);
		Assert.Contains(context.RuntimeLog.LogEntries, entry => entry.Message == "Transition completed.");
	}

	[Fact]
	public async Task Transition_CancelsActiveRoutineBeforeLeavingState()
	{
		var first = new FakeState(
			new StateId("First"),
			handle: (access, _, _) =>
			{
				access.Transition.To(new StateId("Second"), "done");
				return Task.FromResult(true);
			}
		);

		var second = new FakeState(new StateId("Second"));
		var routine = new FakeRoutine(new RoutineId("Routine"));
		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [first, second]);

		await context.StateMachine.StartAsync(first.Id);
		await context.RoutineRunner.StartAsync(routine, access => new FakeAccess(access));
		await context.EventDispatcher.DispatchAsync(new FakeEvent("Next"), CancellationToken.None);

		Assert.Equal(second.Id, context.StateMachine.CurrentStateId);
		Assert.Equal(1, routine.CancelCount);
		Assert.False(context.RoutineRunner.HasActiveRoutine);
	}

	[Fact]
	public async Task ChainedTransitions_AreGuardedAgainstInfiniteLoops()
	{
		var firstId = new StateId("First");
		var secondId = new StateId("Second");
		var first = new FakeState(
			firstId,
			enter: (access, _) =>
			{
				access.Transition.To(secondId);
				return Task.CompletedTask;
			}
		);
		var second = new FakeState(
			secondId,
			enter: (access, _) =>
			{
				access.Transition.To(firstId);
				return Task.CompletedTask;
			}
		);

		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [first, second]);
		await Assert.ThrowsAsync<InvalidOperationException>(() => context.StateMachine.StartAsync(firstId));
	}

	[Fact]
	public async Task StartAsync_ThrowsWhenStartedTwice()
	{
		var state = new FakeState(new StateId("Ready"));
		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [state]);

		await context.StateMachine.StartAsync(state.Id);
		await Assert.ThrowsAsync<InvalidOperationException>(() => context.StateMachine.StartAsync(state.Id));
	}
}
