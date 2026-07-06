namespace Leva.Framework.Core;

/// <summary>
/// Represents a reusable multi-step workflow that can run inside the current runtime context.
/// </summary>
public interface IRoutine<TAccess>
	where TAccess : IAccess
{
	RoutineId Id { get; }
	string Name { get; }
	RoutineStatus Status { get; }

	Task StartAsync(TAccess access, CancellationToken token);
	Task CancelAsync(TAccess access, CancellationToken token);
	Task<bool> HandleAsync(TAccess access, IEvent appEvent, CancellationToken token);
}
