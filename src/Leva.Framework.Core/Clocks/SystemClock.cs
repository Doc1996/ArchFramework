namespace Leva.Framework.Core;

/// <summary>
/// Provides UTC time from the system clock for production runtime execution.
/// </summary>
public sealed class SystemClock : IClock
{
	public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
