using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Binds a behavior to the access factory that creates its state-specific capability surface.
/// </summary>
internal sealed class BehaviorBinding<TAccess>(IBehavior<TAccess> behavior, Func<IAccess, TAccess> createAccess)
	: IBehaviorBinding
	where TAccess : IAccess
{
	public string Name => behavior.Name;

	public Task<bool> HandleAsync(IAccess access, IEvent appEvent, CancellationToken token) =>
		behavior.HandleAsync(createAccess(access), appEvent, token);
}
