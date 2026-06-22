using Leva.Framework.Core;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable behavior fake that records handling calls.
/// </summary>
public sealed class FakeBehavior(string name, Func<FakeAccess, IEvent, CancellationToken, Task<bool>>? handle = null)
	: IBehavior<FakeAccess>
{
	public string Name { get; } = name;
	public int HandleCount { get; private set; }

	public async Task<bool> HandleAsync(FakeAccess access, IEvent appEvent, CancellationToken token)
	{
		HandleCount++;
		if (handle is null)
			return false;

		return await handle(access, appEvent, token);
	}
}
