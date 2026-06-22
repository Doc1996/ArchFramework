namespace Leva.Framework.Core;

/// <summary>
/// Records the lifecycle of one external, long-running, or recoverable command.
/// </summary>
public sealed record CommandEntry(
	CommandId Id,
	string Name,
	CommandStatus Status,
	DateTimeOffset StartedAt,
	DateTimeOffset UpdatedAt,
	DateTimeOffset? FinishedAt = null,
	Error? Error = null,
	IReadOnlyDictionary<string, object?>? Properties = null
);
