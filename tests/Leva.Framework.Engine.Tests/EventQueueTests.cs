using Leva.Framework.Core;
using Leva.Framework.Engine;
using Leva.Framework.Fakes;
using Xunit;

namespace Leva.Framework.Engine.Tests;

public sealed class EventQueueTests
{
	[Fact]
	public async Task DequeueAsync_ReturnsHigherPriorityFirst()
	{
		var queue = CreateQueue();
		var low = new FakeEvent("Low");
		var critical = new FakeEvent("Critical");

		queue.Enqueue(low, EventPriority.Low);
		queue.Enqueue(critical, EventPriority.Critical);

		Assert.Equal(critical, (await queue.DequeueAsync(CancellationToken.None)).AppEvent);
		Assert.Equal(low, (await queue.DequeueAsync(CancellationToken.None)).AppEvent);
	}

	[Fact]
	public void TryDequeue_ReturnsFalseWhenEmpty()
	{
		var queue = CreateQueue();
		Assert.False(queue.TryDequeue(out var queuedEvent));
		Assert.Null(queuedEvent);
	}

	[Fact]
	public async Task EnqueueDelayedAsync_EnqueuesAfterDelay()
	{
		var queue = CreateQueue();
		var appEvent = new FakeEvent("Delayed");

		await queue.EnqueueDelayedAsync(appEvent, TimeSpan.FromMilliseconds(10));
		await Task.Delay(100);

		Assert.True(queue.TryDequeue(out var queuedEvent));
		Assert.Equal(appEvent, queuedEvent?.AppEvent);
	}

	[Fact]
	public async Task Cancel_PreventsDelayedEventFromBeingQueued()
	{
		var queue = CreateQueue();
		var appEvent = new FakeEvent("Delayed");

		await queue.EnqueueDelayedAsync(appEvent, TimeSpan.FromSeconds(1));
		Assert.True(queue.Cancel(appEvent.Id));
		await Task.Delay(50);
		Assert.False(queue.TryDequeue(out _));
	}

	private static EventQueue CreateQueue()
	{
		var clock = new FakeClock();
		return new EventQueue(clock, new RuntimeLog(clock, new FakeTraceSink()));
	}
}
