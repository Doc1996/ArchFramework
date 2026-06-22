using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Maps global events to alarms and optional transitions.
/// </summary>
public interface IAlarmSupervisor
{
	AlarmBoard AlarmBoard { get; }
	Task<bool> HandleAsync(IAccess access, IEvent appEvent, CancellationToken token);
}
