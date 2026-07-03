using Leva.Framework.Core;

namespace Leva.Framework.Sample.StateCounter;

internal sealed class TargetReachedBehavior : IBehavior<CounterAccess>
{
	public string Name => nameof(TargetReachedBehavior);

	public Task<bool> HandleAsync(CounterAccess access, IEvent appEvent, CancellationToken token)
	{
		if (appEvent is not EvaluateCounterEvent)
			return Task.FromResult(false);

		// Behaviors run after the current state declines an event. This keeps cross-state fallback
		// rules separate from individual state handlers.
		access.Model.MarkTargetEvaluated();
		access.StatusBoard.Set(
			new StatusEntry("StateCounter", "TargetCount", access.Model.Count, DateTimeOffset.UtcNow)
		);

		if (access.Model.Count >= CounterModel.WarningLimit)
		{
			access.AlarmBoard.Raise(
				new AlarmEntry(
					AlarmId.New(),
					"Counter reached the sample warning limit.",
					AlarmLevel.Warning,
					IsBlocking: false,
					RaisedAt: DateTimeOffset.UtcNow
				)
			);
		}

		if (access.Model.Count >= CounterModel.Target)
			access.Transition.To(CounterStateIds.Completed, "Target count reached by behavior.");

		return Task.FromResult(true);
	}
}
