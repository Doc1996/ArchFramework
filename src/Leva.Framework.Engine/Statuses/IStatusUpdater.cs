using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Updates status memory from incoming events.
/// </summary>
public interface IStatusUpdater
{
	StatusBoard StatusBoard { get; }
	Task UpdateAsync(IEvent appEvent, CancellationToken token);
}
