using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Default status updater used when an application does not need global status extraction.
/// </summary>
public sealed class NullStatusUpdater(StatusBoard statusBoard) : IStatusUpdater
{
	public StatusBoard StatusBoard { get; } = statusBoard;

	public Task UpdateAsync(IEvent appEvent, CancellationToken token) => Task.CompletedTask;
}
