namespace Leva.Framework.Core;

/// <summary>
/// Describes how serious an active alarm is and how prominently it should be reported.
/// </summary>
public enum AlarmLevel
{
	Info,
	Warning,
	Error,
	Critical,
}

/// <summary>
/// Represents one active alarm or fault condition known by the runtime.
/// </summary>
public sealed record AlarmEntry(
	AlarmId Id,
	string Message,
	AlarmLevel Level,
	bool IsBlocking,
	DateTimeOffset RaisedAt,
	IReadOnlyDictionary<string, object?>? Properties = null
);
