using Leva.Framework.Execution;

namespace Leva.Framework.Sample.StateCounter;

internal sealed record ExecutionItem(string Name, string Status, string? ProgressMessage, double? ProgressPercent)
{
	public static ExecutionItem From(ExecutionEntry entry) =>
		new(entry.Name, entry.Status.ToString(), entry.ProgressMessage, entry.ProgressPercent);
}
