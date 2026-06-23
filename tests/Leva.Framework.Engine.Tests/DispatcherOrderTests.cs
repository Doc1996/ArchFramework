using Leva.Framework.Core;
using Leva.Framework.Engine;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class DispatcherOrderTests
{
	[Fact]
	public async Task Dispatcher_CallsPipelineInDeterministicOrder()
	{
		var calls = new List<string>();
		var clock = new FakeClock();
		var logSink = new FakeLogSink();
		var runtimeLog = new RuntimeLog(clock, logSink);
		var alarmBoard = new AlarmBoard(clock, runtimeLog);
		var statusBoard = new StatusBoard(runtimeLog);

		var statusUpdater = new FakeStatusUpdater(
			statusBoard,
			(_, _) =>
			{
				calls.Add("status");
				return Task.CompletedTask;
			}
		);
		var alarmSupervisor = new FakeAlarmSupervisor(
			alarmBoard,
			(_, _, _) =>
			{
				calls.Add("alarm");
				return Task.FromResult(false);
			}
		);

		var state = new FakeState(
			new StateId("Ready"),
			handle: (_, _, _) =>
			{
				calls.Add("state");
				return Task.FromResult(false);
			}
		);
		var routine = new FakeRoutine(
			new RoutineId("Routine"),
			handle: (_, _, _) =>
			{
				calls.Add("routine");
				return Task.FromResult(false);
			}
		);

		var behavior = new FakeBehavior(
			"Behavior",
			(_, _, _) =>
			{
				calls.Add("behavior");
				return Task.FromResult(true);
			}
		);
		var fake = new FakeContext(clock: clock, logSink: logSink);
		var context = TestContextBuilder.Create(fake, [state], [behavior], alarmSupervisor, statusUpdater);

		await context.StateMachine.StartAsync(state.Id);
		await context.RoutineRunner.StartAsync(routine, access => new FakeAccess(access));
		await context.EventDispatcher.DispatchAsync(new FakeEvent("Event"), CancellationToken.None);
		Assert.Equal(["status", "alarm", "state", "routine", "behavior"], calls);
	}

	[Fact]
	public async Task Dispatcher_DrainsTransitionOnceAfterBehaviorHandles()
	{
		var first = new FakeState(new StateId("First"), handle: (_, _, _) => Task.FromResult(false));
		var second = new FakeState(new StateId("Second"));
		var behavior = new FakeBehavior(
			"TransitionBehavior",
			(access, _, _) =>
			{
				access.Transition.To(second.Id, "behavior handled");
				return Task.FromResult(true);
			}
		);

		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [first, second], [behavior]);
		await context.StateMachine.StartAsync(first.Id);
		await context.EventDispatcher.DispatchAsync(new FakeEvent("Move"), CancellationToken.None);

		Assert.Equal(second.Id, context.StateMachine.CurrentStateId);
		Assert.Equal(1, first.ExitCount);
		Assert.Equal(1, second.EnterCount);
	}

	[Fact]
	public async Task Dispatcher_LogsUnhandledWhenNoHandlerAcceptsEvent()
	{
		var state = new FakeState(new StateId("Ready"), handle: (_, _, _) => Task.FromResult(false));
		var behavior = new FakeBehavior("Behavior", (_, _, _) => Task.FromResult(false));
		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [state], [behavior]);

		await context.StateMachine.StartAsync(state.Id);
		await context.EventDispatcher.DispatchAsync(new FakeEvent("Unhandled"), CancellationToken.None);
		Assert.Contains(context.RuntimeLog.LogEntries, entry => entry.Message == "Event unhandled.");
	}
}
