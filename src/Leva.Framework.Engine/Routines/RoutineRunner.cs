using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Starts, cancels, and routes events to the currently active routine.
/// </summary>
public sealed class RoutineRunner
{
	private IRoutineBinding? _activeRoutine;
	private readonly IAccess _access;
	private readonly RuntimeLog _runtimeLog;

	internal RoutineRunner(IAccess access, RuntimeLog runtimeLog)
	{
		_access = access;
		_runtimeLog = runtimeLog;
	}

	public bool HasActiveRoutine => _activeRoutine is not null;

	public async Task StartAsync<TAccess>(
		IRoutine<TAccess> routine,
		Func<IAccess, TAccess> createAccess,
		CancellationToken token = default
	)
		where TAccess : IAccess
	{
		await CancelAsync(token);
		_activeRoutine = new RoutineBinding<TAccess>(routine, createAccess);
		await _activeRoutine.StartAsync(_access, token);

		_runtimeLog.Add(LogCategory.Routine, "Routine started.", RoutineDetails(_activeRoutine));
		ClearIfFinished();
	}

	public async Task CancelAsync(CancellationToken token = default)
	{
		if (_activeRoutine is null)
			return;

		var routineName = _activeRoutine.Name;
		await _activeRoutine.CancelAsync(token);

		_runtimeLog.Add(LogCategory.Routine, "Routine cancelled.", new { RoutineName = routineName });
		_activeRoutine = null;
	}

	public async Task<bool> HandleAsync(IEvent appEvent, CancellationToken token)
	{
		if (_activeRoutine is null)
			return false;

		var handled = await _activeRoutine.HandleAsync(appEvent, token);
		if (handled)
			_runtimeLog.Add(LogCategory.Routine, "Event handled by routine.", appEvent, RoutineDetails(_activeRoutine));

		ClearIfFinished();
		return handled;
	}

	private void ClearIfFinished()
	{
		if (_activeRoutine?.Status is RoutineStatus.Completed or RoutineStatus.Cancelled or RoutineStatus.Failed)
		{
			_runtimeLog.Add(LogCategory.Routine, "Routine finished.", RoutineDetails(_activeRoutine));
			_activeRoutine = null;
		}
	}

	private static object RoutineDetails(IRoutineBinding routine) => new { RoutineName = routine.Name, routine.Status };
}
