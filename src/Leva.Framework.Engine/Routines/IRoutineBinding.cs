using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Type-erased routine binding used by the routine runner.
/// </summary>
internal interface IRoutineBinding
{
	string Name { get; }
	RoutineStatus Status { get; }

	Task StartAsync(IAccess access, CancellationToken token);
	Task CancelAsync(CancellationToken token);
	Task<bool> HandleAsync(IEvent appEvent, CancellationToken token);
}
