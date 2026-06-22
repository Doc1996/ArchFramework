using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Default alarm supervisor used when an application does not provide global alarm handling.
/// </summary>
public sealed class NullAlarmSupervisor(AlarmBoard alarmBoard) : IAlarmSupervisor
{
	public AlarmBoard AlarmBoard { get; } = alarmBoard;

	public Task<bool> HandleAsync(IAccess access, IEvent appEvent, CancellationToken token) => Task.FromResult(false);
}
