using Leva.Framework.Core;

namespace Leva.Framework.Sample.StateCounter;

internal sealed record AlarmItem(string Id, string Level, string Message, bool IsBlocking)
{
	public static AlarmItem From(AlarmEntry entry) =>
		new(entry.Id.Value.ToString("N"), entry.Level.ToString(), entry.Message, entry.IsBlocking);
}
