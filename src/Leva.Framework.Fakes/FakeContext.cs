namespace Leva.Framework.Fakes;

/// <summary>
/// Provides a ready-to-use fake runtime context for tests.
/// </summary>
public sealed class FakeContext
{
	public FakeClock Clock { get; }
	public FakeTraceSink TraceSink { get; }
	public FakeTransition Transition { get; }
	public FakeAccess Access { get; }
	public FakeEventQueue EventQueue { get; }

	public FakeContext(
		FakeClock? clock = null,
		FakeTraceSink? traceSink = null,
		FakeTransition? transition = null,
		FakeEventQueue? eventQueue = null
	)
	{
		Clock = clock ?? new FakeClock();
		TraceSink = traceSink ?? new FakeTraceSink();
		Transition = transition ?? new FakeTransition();
		Access = new FakeAccess(Transition, TraceSink);
		EventQueue = eventQueue ?? new FakeEventQueue(Clock);
	}
}
