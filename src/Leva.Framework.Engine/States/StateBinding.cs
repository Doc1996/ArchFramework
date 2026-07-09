using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Binds a state to the access factory that creates its state-specific capability surface.
/// </summary>
internal sealed class StateBinding<TAccess>(IState<TAccess> state, Func<IAccess, TAccess> createAccess) : IStateBinding
	where TAccess : IAccess
{
	public StateId Id => state.Id;
	public string Name => state.Name;

	public IAccess CreateAccess(IAccess access) => createAccess(access);

	public Task EnterAsync(IAccess access, CancellationToken token) => state.EnterAsync((TAccess)access, token);

	public Task ExitAsync(IAccess access, CancellationToken token) => state.ExitAsync((TAccess)access, token);

	public Task<bool> HandleAsync(IAccess access, IEvent appEvent, CancellationToken token) =>
		state.HandleAsync((TAccess)access, appEvent, token);
}
