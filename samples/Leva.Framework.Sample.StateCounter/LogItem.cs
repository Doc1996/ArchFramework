using Leva.Framework.Core;

namespace Leva.Framework.Sample.StateCounter;

internal sealed record LogItem(string Category, string Source, string Message)
{
	public static LogItem From(LogEntry entry) => new(entry.Category.ToString(), entry.Source, entry.Message);
}
