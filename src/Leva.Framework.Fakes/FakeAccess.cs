using Leva.Framework.Core;

namespace Leva.Framework.Fakes;

/// <summary>
/// Simple access object for tests that exposes transition and trace capabilities.
/// </summary>
public sealed class FakeAccess : IAccess
{
	public FakeAccess()
		: this(new FakeTransition(), new FakeTraceSink()) { }

	public FakeAccess(IAccess access)
		: this(access.Transition, access.TraceSink) { }

	public FakeAccess(ITransition transition, ITraceSink traceSink)
	{
		Transition = transition;
		TraceSink = traceSink;
	}

	public ITransition Transition { get; }
	public ITraceSink TraceSink { get; }
}
