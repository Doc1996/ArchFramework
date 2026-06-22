namespace Leva.Framework.Core;

/// <summary>
/// Captures a durable runtime point that can be stored and later used to resume execution safely.
/// </summary>
public sealed record Snapshot(StateId StateId, DateTimeOffset CreatedAt, IReadOnlyDictionary<string, object?> Data);
