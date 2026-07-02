using Leva.Framework.Core;

namespace Leva.Framework.Sample.StateCounter;

internal static class CounterStateIds
{
	public static StateId Idle { get; } = new("Idle");
	public static StateId Counting { get; } = new("Counting");
	public static StateId Completed { get; } = new("Completed");
}
