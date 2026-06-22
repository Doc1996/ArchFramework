using Leva.Framework.Core;

namespace Leva.Framework.Fakes;

/// <summary>
/// Controllable clock for deterministic tests.
/// </summary>
public sealed class FakeClock(DateTimeOffset? start = null) : IClock
{
	public DateTimeOffset UtcNow { get; private set; } = start ?? DateTimeOffset.UnixEpoch;

	public void Advance(TimeSpan amount) => UtcNow += amount;

	public void Set(DateTimeOffset utcNow) => UtcNow = utcNow;
}
