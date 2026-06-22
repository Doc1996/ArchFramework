namespace Leva.Framework.Core;

/// <summary>
/// Handles shared fallback behavior for events not consumed by the current state or routine.
/// </summary>
public interface IBehavior<TAccess>
	where TAccess : IAccess
{
	string Name { get; }
	Task<bool> HandleAsync(TAccess access, IEvent appEvent, CancellationToken token);
}
