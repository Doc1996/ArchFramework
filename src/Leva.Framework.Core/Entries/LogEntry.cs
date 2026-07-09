namespace Leva.Framework.Core;

/// <summary>
/// Represents one structured runtime log entry sent to a log sink.
/// </summary>
public sealed record LogEntry(
	string Source,
	string Message,
	LogCategory Category,
	LogLevel Level,
	DateTimeOffset CreatedAt,
	IReadOnlyDictionary<string, object?>? Properties = null
);
