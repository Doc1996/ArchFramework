using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Routes events through the engine pipeline.
/// </summary>
public sealed class EventDispatcher(
	RuntimeLog runtimeLog,
	StateMachine stateMachine,
	IStatusUpdater statusUpdater,
	IAlarmSupervisor alarmSupervisor,
	RoutineRunner routineRunner,
	BehaviorRunner behaviorRunner
)
{
	public async Task DispatchAsync(IEvent appEvent, CancellationToken token)
	{
		runtimeLog.Add(LogCategory.Event, "Event dispatch started.", appEvent);
		await statusUpdater.UpdateAsync(appEvent, token);

		var access = stateMachine.CurrentAccess ?? stateMachine.RuntimeAccess;
		var handled = false;

		if (await alarmSupervisor.HandleAsync(access, appEvent, token))
		{
			runtimeLog.Add(LogCategory.Event, "Event handled by alarm supervisor.", appEvent);
			handled = true;
		}
		else if (await stateMachine.HandleAsync(appEvent, token))
			handled = true;
		else if (await routineRunner.HandleAsync(appEvent, token))
			handled = true;
		else if (await behaviorRunner.HandleAsync(appEvent, token))
			handled = true;

		if (!handled)
			runtimeLog.Add(LogCategory.Event, "Event unhandled.", appEvent);
		await stateMachine.ApplyTransitionsAsync(token);
	}
}
