using Leva.Framework.Core;

namespace Leva.Framework.Sample.StateCounter;

internal sealed class IdleState : IState<CounterAccess>
{
	public StateId Id => CounterStateIds.Idle;
	public string Name => nameof(IdleState);

	public Task EnterAsync(CounterAccess access, CancellationToken token) => Task.CompletedTask;

	public Task ExitAsync(CounterAccess access, CancellationToken token) => Task.CompletedTask;

	public Task<bool> HandleAsync(CounterAccess access, IEvent appEvent, CancellationToken token)
	{
		if (appEvent is not StartCounterEvent)
			return Task.FromResult(false);

		// States request transitions through ITransition instead of owning the StateMachine.
		access.Transition.To(CounterStateIds.Counting, "Counter started.");
		return Task.FromResult(true);
	}
}
