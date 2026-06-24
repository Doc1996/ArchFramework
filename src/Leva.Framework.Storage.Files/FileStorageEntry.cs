namespace Leva.Framework.Storage.Files;

/// <summary>
/// Represents one stored value in the file provider JSON format.
/// </summary>
internal sealed record FileStorageEntry<T>(T Value, long Version, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
