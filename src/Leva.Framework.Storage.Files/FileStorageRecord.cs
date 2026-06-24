namespace Leva.Framework.Storage.Files;

/// <summary>
/// Represents the JSON record stored for one file storage entry.
/// </summary>
internal sealed record FileStorageRecord<T>(
	T Value,
	long Version,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt,
	IReadOnlyDictionary<string, string>? Properties
);
