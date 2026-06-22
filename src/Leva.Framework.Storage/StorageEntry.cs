namespace Leva.Framework.Storage;

/// <summary>
/// Represents one stored value together with provider-owned metadata.
/// </summary>
public readonly record struct StorageEntry<T>(
	T Value,
	StorageVersion Version,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt,
	IReadOnlyDictionary<string, string>? Properties = null
);
