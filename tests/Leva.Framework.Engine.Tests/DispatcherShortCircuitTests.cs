using Leva.Framework.Core;
using Leva.Framework.Engine;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class DispatcherShortCircuitTests
{
	[Fact]
	public async Task Dispatcher_AlarmSupervisorHandlingStopsStateRoutineAndBehavior()
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
				return Task.FromResult(true);
			}
		);

		var state = new FakeState(
			new StateId("Ready"),
			handle: (_, _, _) =>
			{
				calls.Add("state");
				return Task.FromResult(true);
			}
		);

		var routine = new FakeRoutine(
			new RoutineId("Routine"),
			handle: (_, _, _) =>
			{
				calls.Add("routine");
				return Task.FromResult(true);
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
		await context.EventDispatcher.DispatchAsync(new FakeEvent("Alarm"), CancellationToken.None);

		Assert.Equal(["status", "alarm"], calls);
		Assert.DoesNotContain(context.RuntimeLog.LogEntries, entry => entry.Message == "Event unhandled.");
	}

	[Fact]
	public async Task Dispatcher_StateHandlingStopsRoutineAndBehavior()
	{
		var calls = new List<string>();
		var state = new FakeState(
			new StateId("Ready"),
			handle: (_, _, _) =>
			{
				calls.Add("state");
				return Task.FromResult(true);
			}
		);

		var routine = new FakeRoutine(
			new RoutineId("Routine"),
			handle: (_, _, _) =>
			{
				calls.Add("routine");
				return Task.FromResult(true);
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

		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [state], [behavior]);

		await context.StateMachine.StartAsync(state.Id);
		await context.RoutineRunner.StartAsync(routine, access => new FakeAccess(access));
		await context.EventDispatcher.DispatchAsync(new FakeEvent("State"), CancellationToken.None);

		Assert.Equal(["state"], calls);
	}

	[Fact]
	public async Task Dispatcher_RoutineHandlingStopsBehavior()
	{
		var calls = new List<string>();
		var state = new FakeState(new StateId("Ready"), handle: (_, _, _) => Task.FromResult(false));

		var routine = new FakeRoutine(
			new RoutineId("Routine"),
			handle: (_, _, _) =>
			{
				calls.Add("routine");
				return Task.FromResult(true);
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

		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [state], [behavior]);

		await context.StateMachine.StartAsync(state.Id);
		await context.RoutineRunner.StartAsync(routine, access => new FakeAccess(access));
		await context.EventDispatcher.DispatchAsync(new FakeEvent("Routine"), CancellationToken.None);

		Assert.Equal(["routine"], calls);
	}
}
