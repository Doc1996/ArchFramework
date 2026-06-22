using Leva.Framework.Core;

namespace Leva.Framework.Fakes;

/// <summary>
/// Captures one transition request made by fake transition.
/// </summary>
public sealed record FakeTransitionEntry(string Source, StateId? StateId, string? Reason)
{
	public static FakeTransitionEntry To(StateId stateId, string? reason = null) => new("To", stateId, reason);

	public static FakeTransitionEntry Reenter(string? reason = null) => new("Reenter", null, reason);
}
