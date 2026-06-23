using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Fakes.Tests;

public sealed class FakeContextTests
{
	[Fact]
	public void Constructor_CreatesDefaultFakes()
	{
		var context = new FakeContext();

		Assert.NotNull(context.Clock);
		Assert.NotNull(context.LogSink);
		Assert.NotNull(context.Transition);
		Assert.NotNull(context.Access);
		Assert.NotNull(context.EventQueue);

		Assert.Same(context.Transition, context.Access.Transition);
		Assert.Same(context.LogSink, context.Access.LogSink);
	}

	[Fact]
	public void Constructor_UsesProvidedFakeInstances()
	{
		var clock = new FakeClock(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
		var logSink = new FakeLogSink();
		var transition = new FakeTransition();
		var eventQueue = new FakeEventQueue(clock);
		var context = new FakeContext(clock: clock, logSink: logSink, transition: transition, eventQueue: eventQueue);

		Assert.Same(clock, context.Clock);
		Assert.Same(logSink, context.LogSink);
		Assert.Same(transition, context.Transition);
		Assert.Same(eventQueue, context.EventQueue);
	}
}
