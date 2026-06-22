using Leva.Framework.Core;
using Leva.Framework.Engine;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable alarm supervisor fake.
/// </summary>
public sealed class FakeAlarmSupervisor : IAlarmSupervisor
{
	private readonly Func<IAccess, IEvent, CancellationToken, Task<bool>>? _handle;
	public AlarmBoard AlarmBoard { get; }
	public int HandleCount { get; private set; }

	public FakeAlarmSupervisor(
		AlarmBoard alarmBoard,
		Func<IAccess, IEvent, CancellationToken, Task<bool>>? handle = null
	)
	{
		AlarmBoard = alarmBoard;
		_handle = handle;
	}

	public async Task<bool> HandleAsync(IAccess access, IEvent appEvent, CancellationToken token)
	{
		HandleCount++;
		if (_handle is null)
			return false;

		return await _handle(access, appEvent, token);
	}
}
