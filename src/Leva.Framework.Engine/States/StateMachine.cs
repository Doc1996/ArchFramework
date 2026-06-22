using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Owns current state and applies requested transitions.
/// </summary>
public sealed class StateMachine
{
	private const int MaxTransitionCount = 10;

	private readonly IReadOnlyDictionary<StateId, IStateBinding> _states;
	private readonly IAccess _runtimeAccess;
	private readonly QueuedTransition _queuedTransition;
	private readonly RoutineRunner _routineRunner;
	private readonly RuntimeLog _runtimeLog;

	private IStateBinding? _currentState;

	internal StateMachine(
		IEnumerable<IStateBinding> states,
		IAccess runtimeAccess,
		QueuedTransition queuedTransition,
		RoutineRunner routineRunner,
		RuntimeLog runtimeLog
	)
	{
		_states = states.ToDictionary(state => state.Id);
		_runtimeAccess = runtimeAccess;
		_queuedTransition = queuedTransition;
		_routineRunner = routineRunner;
		_runtimeLog = runtimeLog;
	}

	public StateId? CurrentStateId => _currentState?.Id;
	internal IAccess? CurrentAccess { get; private set; }
	internal IAccess RuntimeAccess => _runtimeAccess;

	public async Task StartAsync(StateId stateId, CancellationToken token = default)
	{
		if (_currentState is not null)
			throw new InvalidOperationException("State machine is already started.");

		await EnterAsync(stateId, null, token);
		await ApplyTransitionsAsync(token);
	}

	public async Task<bool> HandleAsync(IEvent appEvent, CancellationToken token)
	{
		if (_currentState is null || CurrentAccess is null)
			return false;

		var handled = await _currentState.HandleAsync(CurrentAccess, appEvent, token);
		if (handled)
			_runtimeLog.Add(LogCategory.State, "Event handled by state.", appEvent, StateDetails(_currentState));

		return handled;
	}

	public async Task ApplyTransitionsAsync(CancellationToken token)
	{
		var transitionCount = 0;
		while (_queuedTransition.TryDequeue(out var targetStateId, out var reason))
		{
			if (++transitionCount > MaxTransitionCount)
				throw new InvalidOperationException("Too many chained transitions.");

			var nextStateId =
				targetStateId
				?? CurrentStateId
				?? throw new InvalidOperationException("Cannot reenter before state machine is started.");
			await TransitionToAsync(nextStateId, reason, token);
		}
	}

	private async Task TransitionToAsync(StateId stateId, string? reason, CancellationToken token)
	{
		var previousStateId = CurrentStateId;
		if (_currentState is not null && CurrentAccess is not null)
		{
			await _routineRunner.CancelAsync(token);
			_runtimeLog.Add(LogCategory.State, "State exited.", StateDetails(_currentState));
			await _currentState.ExitAsync(CurrentAccess, token);
		}

		await EnterAsync(stateId, previousStateId, token);
		_runtimeLog.Add(
			LogCategory.Transition,
			"Transition completed.",
			new
			{
				FromStateId = previousStateId,
				ToStateId = stateId,
				Reason = reason,
			}
		);
	}

	private async Task EnterAsync(StateId stateId, StateId? previousStateId, CancellationToken token)
	{
		if (!_states.TryGetValue(stateId, out var nextState))
			throw new InvalidOperationException($"State '{stateId.Value}' is not registered.");

		_currentState = nextState;
		CurrentAccess = nextState.CreateAccess(_runtimeAccess);

		_runtimeLog.Add(
			LogCategory.State,
			"State entered.",
			StateDetails(nextState),
			new { PreviousStateId = previousStateId }
		);
		await nextState.EnterAsync(CurrentAccess, token);
	}

	private static object StateDetails(IStateBinding state) => new { StateId = state.Id, StateName = state.Name };
}
