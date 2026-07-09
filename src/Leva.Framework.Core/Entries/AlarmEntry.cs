namespace Leva.Framework.Core;

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
