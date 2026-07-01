namespace Leva.Framework.Core;

/// <summary>
/// Provides runtime time so engine logic can be deterministic and replaceable in tests.
/// </summary>
public interface IClock
{
	DateTimeOffset UtcNow { get; }
}
