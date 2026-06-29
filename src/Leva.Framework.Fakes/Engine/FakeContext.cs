namespace Leva.Framework.Fakes;

/// <summary>
/// Provides a ready-to-use fake runtime context for tests.
/// </summary>
public sealed class FakeContext
{
	public FakeClock Clock { get; }
	public FakeLogSink LogSink { get; }
	public FakeTransition Transition { get; }
	public FakeAccess Access { get; }
	public FakeEventQueue EventQueue { get; }

	public FakeContext(
		FakeClock? clock = null,
		FakeLogSink? logSink = null,
		FakeTransition? transition = null,
		FakeEventQueue? eventQueue = null
	)
	{
		Clock = clock ?? new FakeClock();
		LogSink = logSink ?? new FakeLogSink();
		Transition = transition ?? new FakeTransition();
		Access = new FakeAccess(Transition, LogSink);
		EventQueue = eventQueue ?? new FakeEventQueue(Clock);
	}
}
