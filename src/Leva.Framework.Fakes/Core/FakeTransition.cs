using Leva.Framework.Core;

namespace Leva.Framework.Fakes;

/// <summary>
/// In-memory transition capability for tests.
/// </summary>
public sealed class FakeTransition : ITransition
{
	private readonly List<FakeTransitionEntry> _transitionEntries = [];
	public IReadOnlyList<FakeTransitionEntry> TransitionEntries => _transitionEntries;

	public void To(StateId stateId, string? reason = null) =>
		_transitionEntries.Add(FakeTransitionEntry.To(stateId, reason));

	public void Reenter(string? reason = null) => _transitionEntries.Add(FakeTransitionEntry.Reenter(reason));

	public void Clear() => _transitionEntries.Clear();
}
