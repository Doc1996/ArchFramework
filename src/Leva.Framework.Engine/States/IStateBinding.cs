using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Type-erased state binding used by the state machine.
/// </summary>
internal interface IStateBinding
{
	StateId Id { get; }
	string Name { get; }

	IAccess CreateAccess(IAccess access);
	Task EnterAsync(IAccess access, CancellationToken token);
	Task ExitAsync(IAccess access, CancellationToken token);
	Task<bool> HandleAsync(IAccess access, IEvent appEvent, CancellationToken token);
}
