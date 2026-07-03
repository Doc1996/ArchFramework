using Leva.Framework.Core;

namespace Leva.Framework.Sample.StateCounter;

internal sealed record StatusItem(string Source, string Name, string? Value)
{
	public static StatusItem From(StatusEntry entry) => new(entry.Source, entry.Name, entry.Value?.ToString());
}
