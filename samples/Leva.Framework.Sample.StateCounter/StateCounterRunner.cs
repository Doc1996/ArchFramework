using Leva.Framework.Core;
using Leva.Framework.Engine;
using Leva.Framework.Execution;
using Leva.Framework.Fakes;
using FrameworkEventId = Leva.Framework.Core.EventId;

namespace Leva.Framework.Sample.StateCounter;

internal static class StateCounterRunner
{
	public static async Task<StateCounterResult> RunAsync(CancellationToken token = default)
	{
		var clock = new FakeClock(DateTimeOffset.Parse("2026-01-01T00:00:00Z"));
		var logSink = new FakeLogSink();
		var model = new CounterModel();
		var executionBoard = new ExecutionBoard(clock, logSink);
		var executionRunner = new ExecutionRunner(executionBoard);
		Context? context = null;

		// The access factory is the bridge between reusable Engine access and application-specific state data.
		// States receive CounterAccess, not the whole Context, so they stay easy to test and reuse.
		CounterAccess CreateAccess(IAccess access) =>
			new(access.Transition, access.LogSink, model, executionRunner, () => context!.AlarmBoard, () => context!.StatusBoard);

		// ContextBuilder wires the state machine, event loop, runtime log, behaviors, alarms, and status boards.
		context = new ContextBuilder()
			.WithClock(clock)
			.WithLogSink(logSink)
			.AddState(new IdleState(), CreateAccess)
			.AddState(new CountingState(), CreateAccess)
			.AddState(new CompletedState(), CreateAccess)
			.AddBehavior(new TargetReachedBehavior(), CreateAccess)
			.Build();

		await context.StateMachine.StartAsync(CounterStateIds.Idle, token);

		// Events use the shared framework EventId factory. The sample intentionally avoids per-event New() helpers.
		context.EventQueue.Enqueue(new StartCounterEvent(FrameworkEventId.New()));
		context.EventQueue.Enqueue(new IncrementCounterEvent(FrameworkEventId.New()));
		context.EventQueue.Enqueue(new IncrementCounterEvent(FrameworkEventId.New()));

		// Delayed events are useful for timeouts or deferred evaluation. The sample uses a tiny delay so the
		// browser check remains fast while still exercising EventQueue.EnqueueDelayedAsync().
		await context.EventQueue.EnqueueDelayedAsync(
			new EvaluateCounterEvent(FrameworkEventId.New()),
			TimeSpan.FromMilliseconds(25),
			token: token
		);

		for (var i = 0; i < 4; i++)
			await context.EventLoop.RunOneAsync(token);

		var checks = new[]
		{
			Check("workflow reached Completed state", context.StateMachine.CurrentStateId == CounterStateIds.Completed),
			Check("counter handled two increment events", model.Count == CounterModel.Target),
			Check("behavior evaluated the delayed target event", model.TargetEvaluated),
			Check("warning alarm was raised when target was reached", context.AlarmBoard.AlarmEntries.Count == 1),
			Check("status board captured latest target count", context.StatusBoard.StatusEntries.Count == 1),
			Check("execution produced the report", model.Report == "Counter finished with value 2."),
			Check("runtime log captured events, transitions, behavior, alarm, and status", context.RuntimeLog.LogEntries.Count > 0),
			Check("execution board captured one execution", executionBoard.Entries.Count == 1),
		};

		return new StateCounterResult(
			checks.All(check => check.Passed),
			context.StateMachine.CurrentStateId?.Value ?? "None",
			model.Count,
			model.Report,
			context.RuntimeLog.LogEntries.Count,
			executionBoard.Entries.Count,
			context.AlarmBoard.AlarmEntries.Count,
			context.StatusBoard.StatusEntries.Count,
			checks,
			context.RuntimeLog.LogEntries.Select(LogItem.From).ToArray(),
			executionBoard.Entries.Select(ExecutionItem.From).ToArray(),
			context.AlarmBoard.AlarmEntries.Select(AlarmItem.From).ToArray(),
			context.StatusBoard.StatusEntries.Select(StatusItem.From).ToArray()
		);
	}

	private static SampleCheck Check(string name, bool passed) => new(name, passed);
}
