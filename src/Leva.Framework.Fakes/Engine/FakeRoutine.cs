using Leva.Framework.Core;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable routine fake with mutable lifecycle status.
/// </summary>
public sealed class FakeRoutine : IRoutine<FakeAccess>
{
	private readonly Func<FakeAccess, CancellationToken, Task>? _start;
	private readonly Func<FakeAccess, CancellationToken, Task>? _cancel;
	private readonly Func<FakeAccess, IEvent, CancellationToken, Task<bool>>? _handle;

	public RoutineId Id { get; }
	public string Name { get; }
	public RoutineStatus Status { get; private set; } = RoutineStatus.NotStarted;
	public int StartCount { get; private set; }
	public int CancelCount { get; private set; }
	public int HandleCount { get; private set; }

	public FakeRoutine(
		RoutineId id,
		string? name = null,
		Func<FakeAccess, CancellationToken, Task>? start = null,
		Func<FakeAccess, CancellationToken, Task>? cancel = null,
		Func<FakeAccess, IEvent, CancellationToken, Task<bool>>? handle = null
	)
	{
		Id = id;
		Name = name ?? id.Value;
		_start = start;
		_cancel = cancel;
		_handle = handle;
	}

	public async Task StartAsync(FakeAccess access, CancellationToken token)
	{
		StartCount++;
		Status = RoutineStatus.Running;
		if (_start is not null)
			await _start(access, token);
	}

	public async Task CancelAsync(FakeAccess access, CancellationToken token)
	{
		CancelCount++;
		Status = RoutineStatus.Cancelled;
		if (_cancel is not null)
			await _cancel(access, token);
	}

	public async Task<bool> HandleAsync(FakeAccess access, IEvent appEvent, CancellationToken token)
	{
		HandleCount++;
		if (_handle is null)
			return false;

		return await _handle(access, appEvent, token);
	}

	public void Complete() => Status = RoutineStatus.Completed;

	public void Fail() => Status = RoutineStatus.Failed;
}
