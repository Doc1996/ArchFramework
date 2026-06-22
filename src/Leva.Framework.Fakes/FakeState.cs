using Leva.Framework.Core;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable state fake that records lifecycle and handling counts.
/// </summary>
public sealed class FakeState : IState<FakeAccess>
{
	private readonly Func<FakeAccess, CancellationToken, Task>? _enter;
	private readonly Func<FakeAccess, CancellationToken, Task>? _exit;
	private readonly Func<FakeAccess, IEvent, CancellationToken, Task<bool>>? _handle;

	public StateId Id { get; }
	public string Name { get; }
	public int EnterCount { get; private set; }
	public int ExitCount { get; private set; }
	public int HandleCount { get; private set; }
	public List<IEvent> HandledEvents { get; } = [];

	public FakeState(
		StateId id,
		string? name = null,
		Func<FakeAccess, CancellationToken, Task>? enter = null,
		Func<FakeAccess, CancellationToken, Task>? exit = null,
		Func<FakeAccess, IEvent, CancellationToken, Task<bool>>? handle = null
	)
	{
		Id = id;
		Name = name ?? id.Value;
		_enter = enter;
		_exit = exit;
		_handle = handle;
	}

	public async Task EnterAsync(FakeAccess access, CancellationToken token)
	{
		EnterCount++;
		if (_enter is not null)
			await _enter(access, token);
	}

	public async Task ExitAsync(FakeAccess access, CancellationToken token)
	{
		ExitCount++;
		if (_exit is not null)
			await _exit(access, token);
	}

	public async Task<bool> HandleAsync(FakeAccess access, IEvent appEvent, CancellationToken token)
	{
		HandleCount++;
		HandledEvents.Add(appEvent);
		if (_handle is null)
			return false;

		return await _handle(access, appEvent, token);
	}
}
