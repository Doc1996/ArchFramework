using Leva.Framework.Core;

namespace Leva.Framework.Execution;

/// <summary>
/// Stores the current lifecycle snapshot for one execution attempt.
/// </summary>
public sealed record ExecutionEntry(
	ExecutionId Id,
	string Name,
	ExecutionStatus Status,
	DateTimeOffset StartedAt,
	DateTimeOffset UpdatedAt,
	DateTimeOffset? FinishedAt = null,
	string? ProgressMessage = null,
	double? ProgressPercent = null,
	Error? Error = null
);
