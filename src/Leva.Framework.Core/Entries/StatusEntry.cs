namespace Leva.Framework.Core;

/// <summary>
/// Represents one latest-known status value stored by StatusBoard.
/// </summary>
public sealed record StatusEntry(
	string Source,
	string Name,
	object? Value,
	DateTimeOffset UpdatedAt,
	IReadOnlyDictionary<string, object?>? Properties = null
);
