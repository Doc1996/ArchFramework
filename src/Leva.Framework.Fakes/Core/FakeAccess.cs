using Leva.Framework.Core;

namespace Leva.Framework.Fakes;

/// <summary>
/// Simple access object for tests that exposes transition and log capabilities.
/// </summary>
public sealed class FakeAccess(ITransition transition, ILogSink logSink) : IAccess
{
	public FakeAccess()
		: this(new FakeTransition(), new FakeLogSink()) { }

	public FakeAccess(IAccess access)
		: this(access.Transition, access.LogSink) { }

	public ITransition Transition { get; } = transition;
	public ILogSink LogSink { get; } = logSink;
}
