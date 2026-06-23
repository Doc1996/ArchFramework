using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Collects runtime dependencies, state bindings, and behavior bindings, then builds a Context.
/// </summary>
public sealed class ContextBuilder
{
	private readonly List<IStateBinding> _states = [];
	private readonly List<IBehaviorBinding> _behaviors = [];

	private IClock _clock = new SystemClock();
	private ILogSink _logSink = new NullLogSink();
	private IEventQueue? _eventQueue;
	private IAlarmSupervisor? _alarmSupervisor;
	private IStatusUpdater? _statusUpdater;

	public ContextBuilder WithClock(IClock clock)
	{
		_clock = clock;
		return this;
	}

	public ContextBuilder WithLogSink(ILogSink logSink)
	{
		_logSink = logSink;
		return this;
	}

	public ContextBuilder WithEventQueue(IEventQueue eventQueue)
	{
		_eventQueue = eventQueue;
		return this;
	}

	public ContextBuilder WithAlarmSupervisor(IAlarmSupervisor alarmSupervisor)
	{
		_alarmSupervisor = alarmSupervisor;
		return this;
	}

	public ContextBuilder WithStatusUpdater(IStatusUpdater statusUpdater)
	{
		_statusUpdater = statusUpdater;
		return this;
	}

	public ContextBuilder AddState<TAccess>(IState<TAccess> state, Func<IAccess, TAccess> createAccess)
		where TAccess : IAccess
	{
		_states.Add(new StateBinding<TAccess>(state, createAccess));
		return this;
	}

	public ContextBuilder AddBehavior<TAccess>(IBehavior<TAccess> behavior, Func<IAccess, TAccess> createAccess)
		where TAccess : IAccess
	{
		_behaviors.Add(new BehaviorBinding<TAccess>(behavior, createAccess));
		return this;
	}

	public Context Build()
	{
		var runtimeLog = new RuntimeLog(_clock, _logSink);
		var queuedTransition = new QueuedTransition(runtimeLog);
		var runtimeAccess = new RuntimeAccess(queuedTransition, _logSink);
		var eventQueue = _eventQueue ?? new EventQueue(_clock, runtimeLog);
		var alarmBoard = _alarmSupervisor?.AlarmBoard ?? new AlarmBoard(_clock, runtimeLog);
		var statusBoard = _statusUpdater?.StatusBoard ?? new StatusBoard(runtimeLog);
		var commandBoard = new CommandBoard(_clock, runtimeLog);

		var routineRunner = new RoutineRunner(runtimeAccess, runtimeLog);
		var behaviorRunner = new BehaviorRunner(_behaviors, runtimeAccess, runtimeLog);
		var stateMachine = new StateMachine(_states, runtimeAccess, queuedTransition, routineRunner, runtimeLog);
		var alarmSupervisor = _alarmSupervisor ?? new NullAlarmSupervisor(alarmBoard);
		var statusUpdater = _statusUpdater ?? new NullStatusUpdater(statusBoard);

		var eventDispatcher = new EventDispatcher(
			runtimeLog,
			stateMachine,
			statusUpdater,
			alarmSupervisor,
			routineRunner,
			behaviorRunner
		);
		var eventLoop = new EventLoop(eventQueue, eventDispatcher, runtimeLog);

		return new Context(
			_clock,
			_logSink,
			runtimeLog,
			eventQueue,
			stateMachine,
			eventDispatcher,
			eventLoop,
			routineRunner,
			behaviorRunner,
			alarmBoard,
			statusBoard,
			commandBoard
		);
	}

	private sealed class RuntimeAccess(ITransition transition, ILogSink logSink) : IAccess
	{
		public ITransition Transition { get; } = transition;
		public ILogSink LogSink { get; } = logSink;
	}
}
