using Leva.Framework.Core;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable state fake that records lifecycle and handling counts.
/// </summary>
public sealed class FakeState(
	StateId id,
	string? name = null,
	Func<FakeAccess, CancellationToken, Task>? enter = null,
	Func<FakeAccess, CancellationToken, Task>? exit = null,
	Func<FakeAccess, IEvent, CancellationToken, Task<bool>>? handle = null
) : IState<FakeAccess>
{
	private readonly Func<FakeAccess, CancellationToken, Task>? _enter = enter;
	private readonly Func<FakeAccess, CancellationToken, Task>? _exit = exit;
	private readonly Func<FakeAccess, IEvent, CancellationToken, Task<bool>>? _handle = handle;

	public StateId Id { get; } = id;
	public string Name { get; } = name ?? id.Value;
	public int EnterCount { get; private set; }
	public int ExitCount { get; private set; }
	public int HandleCount { get; private set; }
	public List<IEvent> HandledEvents { get; } = [];

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
