using Leva.Framework.Core;
using Leva.Framework.Engine;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class DispatcherAndRoutineTests
{
	[Fact]
	public async Task Dispatcher_UsesBehaviorWhenStateDoesNotHandle()
	{
		var stateId = new StateId("Ready");
		var state = new FakeState(stateId, handle: (_, _, _) => Task.FromResult(false));
		var behavior = new FakeBehavior("Fallback", (_, appEvent, _) => Task.FromResult(appEvent.Name == "Fallback"));
		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [state], [behavior]);

		await context.StateMachine.StartAsync(stateId);
		await context.EventDispatcher.DispatchAsync(new FakeEvent("Fallback"), CancellationToken.None);

		Assert.Equal(1, behavior.HandleCount);
		Assert.Contains(context.RuntimeLog.LogEntries, entry => entry.Message == "Event handled by behavior.");
	}

	[Fact]
	public async Task Dispatcher_StopsWhenAlarmSupervisorHandlesEvent()
	{
		var clock = new FakeClock();
		var logSink = new FakeLogSink();
		var runtimeLog = new RuntimeLog(clock, logSink);
		var alarmBoard = new AlarmBoard(clock, runtimeLog);
		var alarmSupervisor = new FakeAlarmSupervisor(alarmBoard, (_, _, _) => Task.FromResult(true));
		var state = new FakeState(new StateId("Ready"), handle: (_, _, _) => Task.FromResult(true));
		var fake = new FakeContext(clock: clock, logSink: logSink);
		var context = TestContextBuilder.Create(fake, [state], alarmSupervisor: alarmSupervisor);

		await context.StateMachine.StartAsync(state.Id);
		await context.EventDispatcher.DispatchAsync(new FakeEvent("Alarm"), CancellationToken.None);

		Assert.Equal(1, alarmSupervisor.HandleCount);
		Assert.Equal(0, state.HandleCount);
	}

	[Fact]
	public async Task RoutineRunner_StartsHandlesAndClearsCompletedRoutine()
	{
		var state = new FakeState(new StateId("Ready"));
		FakeRoutine? routine = null;

		routine = new(
			new RoutineId("Routine"),
			handle: (_, _, _) =>
			{
				routine!.Complete();
				return Task.FromResult(true);
			}
		);

		var fake = new FakeContext();
		var context = TestContextBuilder.Create(fake, [state]);
		await context.StateMachine.StartAsync(state.Id);
		await context.RoutineRunner.StartAsync(routine, access => new FakeAccess(access));

		Assert.True(await context.RoutineRunner.HandleAsync(new FakeEvent("Run"), CancellationToken.None));
		Assert.False(context.RoutineRunner.HasActiveRoutine);
		Assert.Contains(context.RuntimeLog.LogEntries, entry => entry.Message == "Routine finished.");
	}
}
