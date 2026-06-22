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

/// <summary>
/// Binds a routine to the access factory that creates its state-specific capability surface.
/// </summary>
internal sealed class RoutineBinding<TAccess>(IRoutine<TAccess> routine, Func<IAccess, TAccess> createAccess)
	: IRoutineBinding
	where TAccess : IAccess
{
	private TAccess? _access;

	public string Name => routine.Name;
	public RoutineStatus Status => routine.Status;

	public async Task StartAsync(IAccess access, CancellationToken token)
	{
		_access = createAccess(access);
		await routine.StartAsync(_access, token);
	}

	public Task CancelAsync(CancellationToken token) =>
		_access is null ? Task.CompletedTask : routine.CancelAsync(_access, token);

	public Task<bool> HandleAsync(IEvent appEvent, CancellationToken token) =>
		_access is null ? Task.FromResult(false) : routine.HandleAsync(_access, appEvent, token);
}
