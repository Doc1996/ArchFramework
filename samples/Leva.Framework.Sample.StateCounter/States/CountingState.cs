using Leva.Framework.Core;

namespace Leva.Framework.Sample.StateCounter;

internal sealed class CountingState : IState<CounterAccess>
{
	public StateId Id => CounterStateIds.Counting;
	public string Name => nameof(CountingState);

	public Task EnterAsync(CounterAccess access, CancellationToken token) => Task.CompletedTask;

	public Task ExitAsync(CounterAccess access, CancellationToken token) => Task.CompletedTask;

	public Task<bool> HandleAsync(CounterAccess access, IEvent appEvent, CancellationToken token)
	{
		// State handlers decide whether they consumed each queued IEvent.
		if (appEvent is IncrementCounterEvent)
		{
			access.Model.Increment();
			return Task.FromResult(true);
		}

		if (appEvent is CompleteCounterEvent)
		{
			// Transition requests are logged by the Engine runtime.
			access.Transition.To(CounterStateIds.Completed, "Counter completed.");
			return Task.FromResult(true);
		}

		return Task.FromResult(false);
	}
}
