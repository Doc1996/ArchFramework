namespace Leva.Framework.Core;

/// <summary>
/// Describes how important a runtime log entry is.
/// </summary>
public enum LogLevel
{
	Debug,
	Info,
	Warning,
	Error,
}

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
