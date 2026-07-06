using Leva.Framework.Core;
using Leva.Framework.Engine;

namespace Leva.Framework.Fakes;

/// <summary>
/// Configurable status updater fake.
/// </summary>
public sealed class FakeStatusUpdater(StatusBoard statusBoard, Func<IEvent, CancellationToken, Task>? update = null)
	: IStatusUpdater
{
	private readonly Func<IEvent, CancellationToken, Task>? _update = update;

	public StatusBoard StatusBoard { get; } = statusBoard;
	public int UpdateCount { get; private set; }

	public async Task UpdateAsync(IEvent appEvent, CancellationToken token)
	{
		UpdateCount++;
		if (_update is not null)
			await _update(appEvent, token);
	}
}
