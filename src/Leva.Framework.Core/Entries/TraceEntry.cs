namespace Leva.Framework.Core;

/// <summary>
/// Describes how important a diagnostic trace entry is.
/// </summary>
public enum TraceLevel
{
	Debug,
	Info,
	Warning,
	Error,
}

/// <summary>
/// Represents one diagnostic trace entry sent to a trace sink.
/// </summary>
public sealed record TraceEntry(
	string Source,
	string Message,
	TraceLevel Level,
	DateTimeOffset CreatedAt,
	IReadOnlyDictionary<string, object?>? Properties = null
);
