using Leva.Framework.Core;

namespace Leva.Framework.Engine;

/// <summary>
/// Represents one structured item in the engine runtime log.
/// </summary>
public sealed record LogEntry(
	string Source,
	string Message,
	LogCategory Category,
	TraceLevel Level,
	DateTimeOffset CreatedAt,
	IReadOnlyDictionary<string, object?>? Properties = null
);
