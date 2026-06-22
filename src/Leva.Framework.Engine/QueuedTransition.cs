using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Collects transition requests made through ITransition until the state machine drains them safely.
/// </summary>
internal sealed class QueuedTransition(RuntimeLog runtimeLog) : ITransition
{
	private readonly Lock _lock = new();
	private readonly Queue<(StateId? TargetStateId, string? Reason)> _transitions = new();

	public void To(StateId stateId, string? reason = null)
	{
		lock (_lock)
			_transitions.Enqueue((stateId, reason));

		runtimeLog.Add(
			LogCategory.Transition,
			"Transition requested.",
			new { TargetStateId = stateId, Reason = reason }
		);
	}

	public void Reenter(string? reason = null)
	{
		lock (_lock)
			_transitions.Enqueue((null, reason));
		runtimeLog.Add(LogCategory.Transition, "State reentry requested.", new { Reason = reason });
	}

	public bool TryDequeue(out StateId? targetStateId, out string? reason)
	{
		lock (_lock)
		{
			if (_transitions.TryDequeue(out var transition))
			{
				targetStateId = transition.TargetStateId;
				reason = transition.Reason;
				return true;
			}
		}

		targetStateId = null;
		reason = null;
		return false;
	}
}
